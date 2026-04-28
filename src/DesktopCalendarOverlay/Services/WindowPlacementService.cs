using System.Windows;
using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public sealed class WindowPlacementService(ISettingsStore settingsStore) : IWindowPlacementService
{
    private const string SettingsKey = "window-placement";

    public WindowPlacementState Load() =>
        settingsStore.Read<WindowPlacementState>(SettingsKey) ?? WindowPlacementState.Default;

    public void Apply(Window window, WindowPlacementState placement)
    {
        window.Left = placement.Left;
        window.Top = placement.Top;
        window.Width = Math.Max(720, placement.Width);
        window.Height = Math.Max(480, placement.Height);
        window.Topmost = placement.IsTopmost;
    }

    public void Save(Window window, bool isTopmost)
    {
        if (window.WindowState == WindowState.Minimized)
        {
            return;
        }

        var bounds = window.RestoreBounds;
        if (double.IsInfinity(bounds.Width) || bounds.Width <= 0)
        {
            bounds = new Rect(window.Left, window.Top, window.Width, window.Height);
        }

        settingsStore.Write(
            SettingsKey,
            new WindowPlacementState(bounds.Left, bounds.Top, bounds.Width, bounds.Height, isTopmost));
    }
}
