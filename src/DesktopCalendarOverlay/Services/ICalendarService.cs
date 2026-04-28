using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public interface ICalendarService
{
    Task<IReadOnlyList<CalendarLayer>> GetLayersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        CancellationToken cancellationToken = default);

    Task<CalendarEvent> CreateEventAsync(
        CalendarEvent calendarEvent,
        CancellationToken cancellationToken = default);
}
