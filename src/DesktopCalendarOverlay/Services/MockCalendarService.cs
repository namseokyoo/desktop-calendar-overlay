using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public sealed class MockCalendarService : ICalendarService
{
    private static readonly IReadOnlyList<CalendarLayer> Layers =
    [
        new("primary", "Focus", "#7DD3FC", true, IsPrimary: true),
        new("work", "Work", "#A78BFA", true),
        new("personal", "Personal", "#F9A8D4", true)
    ];

    public Task<IReadOnlyList<CalendarLayer>> GetLayersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Layers);

    public Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var events = new List<CalendarEvent>
        {
            Create("standup", "work", "Team standup", today, 9, 30, 30),
            Create("design", "primary", "Overlay shell spike", today, 13, 0, 90),
            Create("review", "personal", "Weekly planning", today.AddDays(2), 16, 0, 60),
            CreateAllDay("focus-day", "primary", "Deep work day", today.AddDays(5))
        };

        var filtered = events
            .Where(calendarEvent => DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime) >= fromInclusive)
            .Where(calendarEvent => DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime) < toExclusive)
            .OrderBy(calendarEvent => calendarEvent.StartsAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<CalendarEvent>>(filtered);
    }

    public Task<CalendarEvent> CreateEventAsync(
        CalendarEvent calendarEvent,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(calendarEvent);

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
        int durationMinutes)
    {
        var start = date.ToDateTime(new TimeOnly(hour, minute));
        var offset = TimeZoneInfo.Local.GetUtcOffset(start);
        var startsAt = new DateTimeOffset(start, offset);

        return new CalendarEvent(
            id,
            layerId,
            title,
            startsAt,
            startsAt.AddMinutes(durationMinutes));
    }
}
