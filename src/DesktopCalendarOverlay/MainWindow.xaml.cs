using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DesktopCalendarOverlay.Services;
using DesktopCalendarOverlay.ViewModels;

namespace DesktopCalendarOverlay;

public partial class MainWindow : Window
{
    private readonly IWindowPlacementService _windowPlacementService;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        var settingsStore = new JsonSettingsStore();
        _windowPlacementService = new WindowPlacementService(settingsStore);
        _viewModel = new MainViewModel(new MockCalendarService());
        DataContext = _viewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var placement = _windowPlacementService.Load();
        _windowPlacementService.Apply(this, placement);
        await _viewModel.InitializeAsync(placement.IsTopmost);
    }

    private void OnClosing(object? sender, CancelEventArgs e) =>
        _windowPlacementService.Save(this, _viewModel.IsTopmost);

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
