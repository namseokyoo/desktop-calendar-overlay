namespace DesktopCalendarOverlay.Models;

public sealed record CalendarEvent(
    string Id,
    string CalendarLayerId,
    string Title,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsAllDay = false,
    string? Location = null);
