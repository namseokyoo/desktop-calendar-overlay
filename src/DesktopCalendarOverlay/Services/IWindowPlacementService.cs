using System.Windows;
using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public interface IWindowPlacementService
{
    WindowPlacementState Load();

    void Apply(Window window, WindowPlacementState placement);

    void Save(Window window, bool isPositionLocked);
}
