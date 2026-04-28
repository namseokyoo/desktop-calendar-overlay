using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.ViewModels;

public sealed class CalendarDayViewModel
{
    public required DateOnly Date { get; init; }


    public required string DayNumber { get; init; }

    public required bool IsInCurrentMonth { get; init; }

    public required bool IsToday { get; init; }

    public required bool IsSelected { get; init; }

    public required IReadOnlyList<CalendarEvent> Events { get; init; }

    public bool HasEvents => Events.Count > 0;

    public IEnumerable<CalendarEvent> PreviewEvents => Events.Take(4);

    public int MoreEventCount => Math.Max(0, Events.Count - 4);

    public bool HasMoreEvents => MoreEventCount > 0;
}
