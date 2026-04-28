using System.Windows;
using System.Windows.Media;
using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay.Services;

public static class ThemePaletteService
{
    public static void Apply(string themeName)
    {
        var palette = themeName switch
        {
            CalendarThemeNames.IvoryEditorial => new Palette(
                "#EDEFE8DD", "#DFF8F3E8", "#BFF4E7CF", "#F5FFF7E8", "#667A1F1F",
                "#A09A3412", "#FF1F2933", "#FF475569", "#AA64748B", "#FF991B1B", "#33991B1B"),
            CalendarThemeNames.MidnightBlue => new Palette(
                "#E6091020", "#AA111C35", "#66111C35", "#DD0B1226", "#6686A5FF",
                "#CC8FB3FF", "#FFEFF6FF", "#FFB6C7E6", "#8890A4C5", "#FF8FB3FF", "#338FB3FF"),
            _ => new Palette(
                "#D91A2233", "#99283447", "#55283447", "#CC111827", "#55FFFFFF",
                "#AA7DD3FC", "#FFF8FAFC", "#FFCBD5E1", "#8894A3B8", "#FF7DD3FC", "#337DD3FC")
        };

        SetBrushColor("OverlayBackgroundBrush", palette.Background);
        SetBrushColor("OverlaySurfaceBrush", palette.Surface);
        SetBrushColor("OverlaySurfaceMutedBrush", palette.SurfaceMuted);
        SetBrushColor("OverlaySurfaceStrongBrush", palette.SurfaceStrong);
        SetBrushColor("OverlayBorderBrush", palette.Border);
        SetBrushColor("OverlayBorderStrongBrush", palette.BorderStrong);
        SetBrushColor("OverlayTextPrimaryBrush", palette.TextPrimary);
        SetBrushColor("OverlayTextSecondaryBrush", palette.TextSecondary);
        SetBrushColor("OverlayTextMutedBrush", palette.TextMuted);
        SetBrushColor("OverlayAccentBrush", palette.Accent);
        SetBrushColor("OverlayAccentSoftBrush", palette.AccentSoft);
    }

    private static void SetBrushColor(string resourceKey, string color)
    {
        if (Application.Current?.Resources[resourceKey] is SolidColorBrush brush)
        {
            brush.Color = (Color)ColorConverter.ConvertFromString(color);
        }
    }

    private sealed record Palette(
        string Background,
        string Surface,
        string SurfaceMuted,
        string SurfaceStrong,
        string Border,
        string BorderStrong,
        string TextPrimary,
        string TextSecondary,
        string TextMuted,
        string Accent,
        string AccentSoft);
}
