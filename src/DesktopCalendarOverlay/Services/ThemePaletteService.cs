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
                "#F2F8F4EA", "#E6FFF9EE", "#BFE8E0CF", "#FAFFFDF4", "#7A7C3F2D",
                "#B85F6F52", "#FF172033", "#FF435066", "#AA64748B", "#FF8B3A2F", "#338B3A2F"),
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
        if (System.Windows.Application.Current is null)
        {
            return;
        }

        var parsedColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
        if (System.Windows.Application.Current.Resources[resourceKey] is SolidColorBrush brush && !brush.IsFrozen)
        {
            brush.Color = parsedColor;
            return;
        }

        System.Windows.Application.Current.Resources[resourceKey] = new SolidColorBrush(parsedColor);
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
