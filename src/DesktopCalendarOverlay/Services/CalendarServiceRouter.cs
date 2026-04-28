using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public sealed class CalendarServiceRouter(
    GoogleCalendarService googleCalendarService,
    MockCalendarService mockCalendarService) : ICalendarService, IGoogleCalendarIntegration
{
    public string ClientSecretPath => googleCalendarService.ClientSecretPath;

    public string TokenDirectory => googleCalendarService.TokenDirectory;

    public bool IsClientSecretAvailable => googleCalendarService.IsClientSecretAvailable;

    public bool HasStoredToken => googleCalendarService.HasStoredToken;

    public bool IsUsingGoogle => googleCalendarService.IsUsingGoogle;

    public Task ConnectAsync(CancellationToken cancellationToken = default) =>
        googleCalendarService.ConnectAsync(cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken = default) =>
        googleCalendarService.DisconnectAsync(cancellationToken);

    public async Task<IReadOnlyList<CalendarLayer>> GetLayersAsync(CancellationToken cancellationToken = default)
    {
        if (!IsUsingGoogle)
        {
            return await mockCalendarService.GetLayersAsync(cancellationToken);
        }

        var layers = await googleCalendarService.GetLayersAsync(cancellationToken);
        return layers.Count == 0 ? await mockCalendarService.GetLayersAsync(cancellationToken) : layers;
    }

    public async Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        CancellationToken cancellationToken = default)
    {
        if (!IsUsingGoogle)
        {
            return await mockCalendarService.GetEventsAsync(fromInclusive, toExclusive, cancellationToken);
        }

        return await googleCalendarService.GetEventsAsync(fromInclusive, toExclusive, cancellationToken);
    }

    public Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default) =>
        IsUsingGoogle
            ? googleCalendarService.CreateEventAsync(calendarEvent, cancellationToken)
            : mockCalendarService.CreateEventAsync(calendarEvent, cancellationToken);

    public Task SetLayerVisibilityAsync(string calendarLayerId, bool isVisible, CancellationToken cancellationToken = default) =>
        IsUsingGoogle
            ? googleCalendarService.SetLayerVisibilityAsync(calendarLayerId, isVisible, cancellationToken)
            : Task.CompletedTask;
}
