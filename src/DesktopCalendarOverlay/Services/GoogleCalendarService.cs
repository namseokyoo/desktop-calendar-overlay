using System.Globalization;
using System.IO;
using DesktopCalendarOverlay.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleCalendarApi = Google.Apis.Calendar.v3.CalendarService;

namespace DesktopCalendarOverlay.Services;

public sealed class GoogleCalendarService(ISettingsStore settingsStore) : ICalendarService, IGoogleCalendarIntegration
{
    private const string ApplicationName = "Desktop Calendar Overlay";
    private const string LayerVisibilityKey = "google-calendar-layer-visibility";

    private static readonly string[] Scopes =
    [
        CalendarService.Scope.CalendarReadonly,
        CalendarService.Scope.CalendarEvents
    ];

    public string ClientSecretPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopCalendarOverlay",
        "google-oauth-client.json");

    public string TokenDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopCalendarOverlay",
        "google-token-store");

    public bool IsClientSecretAvailable => File.Exists(ClientSecretPath);

    public bool HasStoredToken => Directory.Exists(TokenDirectory) &&
        Directory.EnumerateFiles(TokenDirectory, "*", SearchOption.AllDirectories).Any();

    public bool IsUsingGoogle => IsClientSecretAvailable && HasStoredToken;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsClientSecretAvailable)
        {
            throw new FileNotFoundException("Google OAuth desktop client JSON was not found.", ClientSecretPath);
        }

        await CreateCalendarServiceAsync(forceAuthorization: true, cancellationToken);
        AppDiagnostics.Info("Google Calendar authorization completed.");
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(TokenDirectory))
        {
            Directory.Delete(TokenDirectory, recursive: true);
        }

        AppDiagnostics.Info("Google Calendar local token store deleted.");
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<CalendarLayer>> GetLayersAsync(CancellationToken cancellationToken = default)
    {
        if (!IsUsingGoogle)
        {
            return [];
        }

        using var service = await CreateCalendarServiceAsync(forceAuthorization: false, cancellationToken);
        var visibility = LoadVisibilityOverrides();
        var request = service.CalendarList.List();
        request.MinAccessRole = CalendarListResource.ListRequest.MinAccessRoleEnum.Reader;

        var calendars = new List<CalendarLayer>();
        string? pageToken = null;
        do
        {
            request.PageToken = pageToken;
            var page = await request.ExecuteAsync(cancellationToken);
            foreach (var item in page.Items ?? [])
            {
                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    continue;
                }

                var defaultVisible = item.Selected ?? true;
                calendars.Add(new CalendarLayer(
                    item.Id,
                    string.IsNullOrWhiteSpace(item.SummaryOverride) ? item.Summary ?? item.Id : item.SummaryOverride,
                    NormalizeColor(item.BackgroundColor),
                    visibility.TryGetValue(item.Id, out var isVisible) ? isVisible : defaultVisible,
                    item.Primary ?? false));
            }

            pageToken = page.NextPageToken;
        }
        while (!string.IsNullOrEmpty(pageToken));

        return calendars
            .OrderByDescending(layer => layer.IsPrimary)
            .ThenBy(layer => layer.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        CancellationToken cancellationToken = default)
    {
        if (!IsUsingGoogle)
        {
            return [];
        }

        using var service = await CreateCalendarServiceAsync(forceAuthorization: false, cancellationToken);
        var layers = await GetLayersAsync(cancellationToken);
        var visibleLayers = layers.Where(layer => layer.IsVisible).ToList();
        var events = new List<CalendarEvent>();

        var rangeStart = fromInclusive.ToDateTime(TimeOnly.MinValue);
        var rangeEnd = toExclusive.ToDateTime(TimeOnly.MinValue);
        var timeMin = new DateTimeOffset(rangeStart, TimeZoneInfo.Local.GetUtcOffset(rangeStart));
        var timeMax = new DateTimeOffset(rangeEnd, TimeZoneInfo.Local.GetUtcOffset(rangeEnd));

        foreach (var layer in visibleLayers)
        {
            var request = service.Events.List(layer.Id);
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.TimeMinDateTimeOffset = timeMin;
            request.TimeMaxDateTimeOffset = timeMax;
            request.ShowDeleted = false;

            string? pageToken = null;
            do
            {
                request.PageToken = pageToken;
                var page = await request.ExecuteAsync(cancellationToken);
                foreach (var item in page.Items ?? [])
                {
                    var mapped = MapEvent(layer.Id, item);
                    if (mapped is not null)
                    {
                        events.Add(mapped);
                    }
                }

                pageToken = page.NextPageToken;
            }
            while (!string.IsNullOrEmpty(pageToken));
        }

        return events
            .OrderBy(calendarEvent => calendarEvent.IsAllDay ? 0 : 1)
            .ThenBy(calendarEvent => calendarEvent.StartsAt)
            .ToList();
    }

    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        if (!IsUsingGoogle)
        {
            throw new InvalidOperationException("Connect Google Calendar before creating events.");
        }

        using var service = await CreateCalendarServiceAsync(forceAuthorization: false, cancellationToken);
        var inserted = await service.Events.Insert(BuildEventBody(calendarEvent), calendarEvent.CalendarLayerId).ExecuteAsync(cancellationToken);
        return MapEvent(calendarEvent.CalendarLayerId, inserted) ?? calendarEvent with { Id = inserted.Id ?? string.Empty };
    }

    public async Task<CalendarEvent> UpdateEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        if (!IsUsingGoogle)
        {
            throw new InvalidOperationException("Connect Google Calendar before updating events.");
        }

        if (string.IsNullOrWhiteSpace(calendarEvent.Id))
        {
            throw new InvalidOperationException("Calendar event id is required before updating events.");
        }

        using var service = await CreateCalendarServiceAsync(forceAuthorization: false, cancellationToken);
        var updated = await service.Events.Patch(BuildEventBody(calendarEvent), calendarEvent.CalendarLayerId, calendarEvent.Id).ExecuteAsync(cancellationToken);
        return MapEvent(calendarEvent.CalendarLayerId, updated) ?? calendarEvent;
    }

    public async Task DeleteEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        if (!IsUsingGoogle)
        {
            throw new InvalidOperationException("Connect Google Calendar before deleting events.");
        }

        if (string.IsNullOrWhiteSpace(calendarEvent.Id))
        {
            throw new InvalidOperationException("Calendar event id is required before deleting events.");
        }

        using var service = await CreateCalendarServiceAsync(forceAuthorization: false, cancellationToken);
        await service.Events.Delete(calendarEvent.CalendarLayerId, calendarEvent.Id).ExecuteAsync(cancellationToken);
    }

    private static Event BuildEventBody(CalendarEvent calendarEvent)
    {
        var requestBody = new Event
        {
            Summary = calendarEvent.Title,
            Location = calendarEvent.Location
        };

        if (calendarEvent.IsAllDay)
        {
            requestBody.Start = new EventDateTime
            {
                Date = calendarEvent.StartsAt.LocalDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            requestBody.End = new EventDateTime
            {
                Date = calendarEvent.EndsAt.LocalDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }
        else
        {
            requestBody.Start = new EventDateTime
            {
                DateTimeDateTimeOffset = calendarEvent.StartsAt,
                TimeZone = TimeZoneInfo.Local.Id
            };
            requestBody.End = new EventDateTime
            {
                DateTimeDateTimeOffset = calendarEvent.EndsAt,
                TimeZone = TimeZoneInfo.Local.Id
            };
        }

        return requestBody;
    }

    public Task SetLayerVisibilityAsync(string calendarLayerId, bool isVisible, CancellationToken cancellationToken = default)
    {
        var visibility = LoadVisibilityOverrides();
        visibility[calendarLayerId] = isVisible;
        settingsStore.Write(LayerVisibilityKey, visibility);
        AppDiagnostics.Info($"Google calendar layer visibility saved for calendar '{calendarLayerId}': {isVisible}");
        return Task.CompletedTask;
    }

    private async Task<GoogleCalendarApi> CreateCalendarServiceAsync(bool forceAuthorization, CancellationToken cancellationToken)
    {
        if (!IsClientSecretAvailable)
        {
            throw new FileNotFoundException("Google OAuth desktop client JSON was not found.", ClientSecretPath);
        }

        await using var stream = File.OpenRead(ClientSecretPath);
        var secrets = GoogleClientSecrets.FromStream(stream).Secrets;
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            Scopes,
            "default-user",
            cancellationToken,
            new FileDataStore(TokenDirectory, fullPath: true));

        if (forceAuthorization && string.IsNullOrWhiteSpace(credential.Token.RefreshToken) && !HasStoredToken)
        {
            AppDiagnostics.Info("Google authorization completed without a refresh token; existing browser consent may have been reused.");
        }

        return new GoogleCalendarApi(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });
    }

    private Dictionary<string, bool> LoadVisibilityOverrides() =>
        settingsStore.Read<Dictionary<string, bool>>(LayerVisibilityKey) ?? [];

    private static CalendarEvent? MapEvent(string layerId, Event item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            return null;
        }

        var (startsAt, isAllDay) = ReadEventTime(item.Start, defaultStart: DateTimeOffset.Now);
        var (endsAt, _) = ReadEventTime(item.End, defaultStart: startsAt.AddMinutes(30));

        return new CalendarEvent(
            item.Id,
            layerId,
            string.IsNullOrWhiteSpace(item.Summary) ? "(No title)" : item.Summary,
            startsAt,
            endsAt,
            isAllDay,
            item.Location);
    }

    private static (DateTimeOffset Time, bool IsAllDay) ReadEventTime(EventDateTime? eventDateTime, DateTimeOffset defaultStart)
    {
        if (eventDateTime is null)
        {
            return (defaultStart, false);
        }

        if (!string.IsNullOrWhiteSpace(eventDateTime.Date))
        {
            var date = DateOnly.Parse(eventDateTime.Date, CultureInfo.InvariantCulture);
            var localDateTime = date.ToDateTime(TimeOnly.MinValue);
            return (new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime)), true);
        }

        if (eventDateTime.DateTimeDateTimeOffset is DateTimeOffset dateTimeOffset)
        {
            return (dateTimeOffset.ToLocalTime(), false);
        }

        return (defaultStart, false);
    }

    private static string NormalizeColor(string? color) =>
        string.IsNullOrWhiteSpace(color) ? "#7DD3FC" : color;
}
