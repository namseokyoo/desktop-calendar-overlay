using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public sealed class MockCalendarService : ICalendarService
{
    private static readonly IReadOnlyList<CalendarLayer> Layers =
    [
        new("primary", "Focus", "#7DD3FC", true, isPrimary: true),
        new("work", "Work", "#A78BFA", true),
        new("personal", "Personal", "#F9A8D4", true),
        new("home", "Home", "#86EFAC", false)
    ];

    public Task<IReadOnlyList<CalendarLayer>> GetLayersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Layers);

    public Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        CancellationToken cancellationToken = default)
    {
        var visibleLayerIds = Layers.Where(layer => layer.IsVisible).Select(layer => layer.Id).ToHashSet(StringComparer.Ordinal);
        var events = new List<CalendarEvent>();

        for (var date = fromInclusive; date < toExclusive; date = date.AddDays(1))
        {
            events.AddRange(CreateMockEventsForDate(date));
        }

        events.AddRange(_createdEvents.Where(calendarEvent =>
            DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime) >= fromInclusive &&
            DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime) < toExclusive));

        var filtered = events
            .Where(calendarEvent => visibleLayerIds.Contains(calendarEvent.CalendarLayerId))
            .OrderBy(calendarEvent => calendarEvent.IsAllDay ? 0 : 1)
            .ThenBy(calendarEvent => calendarEvent.StartsAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<CalendarEvent>>(filtered);
    }

    private readonly List<CalendarEvent> _createdEvents = [];

    public Task<CalendarEvent> CreateEventAsync(
        CalendarEvent calendarEvent,
        CancellationToken cancellationToken = default)
    {
        var created = calendarEvent with
        {
            Id = string.IsNullOrWhiteSpace(calendarEvent.Id)
                ? $"mock-created-{Guid.NewGuid():N}"
                : calendarEvent.Id
        };
        _createdEvents.Add(created);
        return Task.FromResult(created);
    }

    private static IEnumerable<CalendarEvent> CreateMockEventsForDate(DateOnly date)
    {
        var seed = date.Day + (date.Month * 3) + date.Year;

        if (date.Day == 1)
        {
            yield return Create($"month-plan-{date:yyyyMMdd}", "primary", "Month planning", date, 9, 0, 45, "Desk");
        }

        if (date.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday && seed % 2 == 0)
        {
            yield return Create($"standup-{date:yyyyMMdd}", "work", "Team standup", date, 9, 30, 30, "Meet");
        }

        if (seed % 3 == 0)
        {
            yield return Create($"focus-{date:yyyyMMdd}", "primary", "Focus block", date, 11, 0, 90, "Deep work");
        }

        if (seed % 5 == 0)
        {
            yield return Create($"review-{date:yyyyMMdd}", "work", "Prototype review", date, 14, 30, 45, "Conference room");
        }

        if (seed % 7 == 0)
        {
            yield return Create($"personal-{date:yyyyMMdd}", "personal", "Personal errand", date, 17, 30, 40, "Downtown");
        }

        if (date.Day == 15)
        {
            yield return CreateAllDay($"deep-work-{date:yyyyMMdd}", "primary", "Deep work day", date);
            yield return Create($"sync-{date:yyyyMMdd}", "work", "Partner sync", date, 13, 0, 60, "Meet");
            yield return Create($"walk-{date:yyyyMMdd}", "personal", "Coffee walk", date, 16, 15, 30, "Park");
        }
    }

    private static CalendarEvent CreateAllDay(string id, string layerId, string title, DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var offset = TimeZoneInfo.Local.GetUtcOffset(start);
        var startsAt = new DateTimeOffset(start, offset);

        return new CalendarEvent(id, layerId, title, startsAt, startsAt.AddDays(1), IsAllDay: true);
    }

    private static CalendarEvent Create(
        string id,
        string layerId,
        string title,
        DateOnly date,
        int hour,
        int minute,
        int durationMinutes,
        string? location = null)
    {
        var start = date.ToDateTime(new TimeOnly(hour, minute));
        var offset = TimeZoneInfo.Local.GetUtcOffset(start);
        var startsAt = new DateTimeOffset(start, offset);

        return new CalendarEvent(
            id,
            layerId,
            title,
            startsAt,
            startsAt.AddMinutes(durationMinutes),
            Location: location);
    }
}
