namespace DesktopCalendarOverlay.Models;

public sealed record CalendarEvent(
    string Id,
    string CalendarLayerId,
    string Title,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsAllDay = false,
    string? Location = null,
    string LayerColorHex = "#7DD3FC")
{
    public string TimeDisplay => IsAllDay
        ? "All day"
        : $"{StartsAt:t}–{EndsAt:t}";
}
