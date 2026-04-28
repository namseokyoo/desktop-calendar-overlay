using System.Windows;
using System.Windows.Controls;
using DesktopCalendarOverlay.Models;
using DesktopCalendarOverlay.ViewModels;

namespace DesktopCalendarOverlay;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnLayerColorSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { DataContext: CalendarLayer layer } && DataContext is MainViewModel viewModel)
        {
            if (viewModel.UpdateLayerColorCommand.CanExecute(layer))
            {
                viewModel.UpdateLayerColorCommand.Execute(layer);
            }
        }
    }
}
