using System.Windows;
using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public sealed class WindowPlacementService(ISettingsStore settingsStore) : IWindowPlacementService
{
    private const string SettingsKey = "window-placement";
    private const double DefaultMinWidth = 720;
    private const double DefaultMinHeight = 480;

    public WindowPlacementState Load() =>
        settingsStore.Read<WindowPlacementState>(SettingsKey) ?? WindowPlacementState.Default;

    public void Apply(Window window, WindowPlacementState placement)
    {
        var width = IsFinitePositive(placement.Width) ? Math.Max(DefaultMinWidth, placement.Width) : WindowPlacementState.Default.Width;
        var height = IsFinitePositive(placement.Height) ? Math.Max(DefaultMinHeight, placement.Height) : WindowPlacementState.Default.Height;
        var left = double.IsFinite(placement.Left) ? placement.Left : WindowPlacementState.Default.Left;
        var top = double.IsFinite(placement.Top) ? placement.Top : WindowPlacementState.Default.Top;

        if (!IsMostlyOnVirtualScreen(left, top, width, height))
        {
            left = WindowPlacementState.Default.Left;
            top = WindowPlacementState.Default.Top;
            width = WindowPlacementState.Default.Width;
            height = WindowPlacementState.Default.Height;
        }

        window.Left = left;
        window.Top = top;
        window.Width = width;
        window.Height = height;
    }

    public void Save(Window window, bool isPositionLocked)
    {
        if (window.WindowState == WindowState.Minimized)
        {
            return;
        }

        var bounds = window.WindowState == WindowState.Normal
            ? new Rect(window.Left, window.Top, window.Width, window.Height)
            : window.RestoreBounds;

        if (double.IsInfinity(bounds.Width) || bounds.Width <= 0 || bounds.Height <= 0)
        {
            bounds = new Rect(window.Left, window.Top, window.Width, window.Height);
        }

        settingsStore.Write(
            SettingsKey,
            new WindowPlacementState(bounds.Left, bounds.Top, bounds.Width, bounds.Height, isPositionLocked));
    }

    private static bool IsFinitePositive(double value) => double.IsFinite(value) && value > 0;

    private static bool IsMostlyOnVirtualScreen(double left, double top, double width, double height)
    {
        var virtualScreen = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
        var proposed = new Rect(left, top, width, height);
        proposed.Intersect(virtualScreen);

        return proposed.Width >= Math.Min(200, width) && proposed.Height >= Math.Min(160, height);
    }
}
