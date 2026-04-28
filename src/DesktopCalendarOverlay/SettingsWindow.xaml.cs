using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DesktopCalendarOverlay.Models;
using DesktopCalendarOverlay.ViewModels;
using Forms = System.Windows.Forms;
using DrawingColor = System.Drawing.Color;

namespace DesktopCalendarOverlay;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnLayerColorButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { DataContext: CalendarLayer layer } || DataContext is not MainViewModel viewModel)
        {
            return;
        }

        using var dialog = new Forms.ColorDialog
        {
            AllowFullOpen = true,
            AnyColor = true,
            FullOpen = true,
            Color = ParseDrawingColor(layer.ColorHex)
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        layer.ColorHex = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
        if (viewModel.UpdateLayerColorCommand.CanExecute(layer))
        {
            viewModel.UpdateLayerColorCommand.Execute(layer);
        }
    }

    private static DrawingColor ParseDrawingColor(string colorHex)
    {
        var normalized = colorHex.Trim().TrimStart('#');
        if (normalized.Length == 6 && int.TryParse(normalized, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            return DrawingColor.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        }

        return DrawingColor.FromArgb(0x7D, 0xD3, 0xFC);
    }
}
