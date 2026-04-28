namespace DesktopCalendarOverlay.Models;

public sealed record CalendarOverlaySettings(
    string EventDisplayMode = CalendarEventDisplayModes.TimeFirst,
    double OverlayOpacity = 1.0,
    string ThemeName = CalendarThemeNames.AcrylicDark);

public static class CalendarEventDisplayModes
{
    public const string TimeFirst = "Time · Event";
    public const string EventFirst = "Event · Time";
}

public static class CalendarThemeNames
{
    public const string AcrylicDark = "Acrylic Dark";
    public const string IvoryEditorial = "Ivory Editorial";
    public const string MidnightBlue = "Midnight Blue";
}
