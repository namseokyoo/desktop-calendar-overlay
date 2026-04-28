namespace DesktopCalendarOverlay.Models;

public sealed record WindowPlacementState(
    double Left,
    double Top,
    double Width,
    double Height,
    bool IsTopmost)
{
    public static WindowPlacementState Default => new(80, 80, 1180, 760, false);
}
