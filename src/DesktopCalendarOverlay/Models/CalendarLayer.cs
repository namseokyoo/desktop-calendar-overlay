namespace DesktopCalendarOverlay.Models;

public sealed record CalendarLayer(
    string Id,
    string Name,
    string ColorHex,
    bool IsVisible,
    bool IsPrimary = false);
