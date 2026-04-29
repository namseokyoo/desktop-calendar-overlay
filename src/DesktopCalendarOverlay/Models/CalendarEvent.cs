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

    public bool IsUpcoming => !IsAllDay && StartsAt >= DateTimeOffset.Now && StartsAt <= DateTimeOffset.Now.AddHours(24);

    public bool HasLocation => !string.IsNullOrWhiteSpace(Location);
}
