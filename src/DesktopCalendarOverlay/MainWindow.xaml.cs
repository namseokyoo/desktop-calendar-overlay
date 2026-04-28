using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
        var googleCalendarService = new GoogleCalendarService(settingsStore);
        var calendarService = new CalendarServiceRouter(googleCalendarService, new MockCalendarService());
        _viewModel = new MainViewModel(calendarService, calendarService);
        _viewModel.OpenSettingsRequested += OnOpenSettingsRequested;
        DataContext = _viewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            AppDiagnostics.Info("Main window loaded; applying placement and loading calendar preview.");
            var placement = _windowPlacementService.Load();
            _windowPlacementService.Apply(this, placement);
            await _viewModel.InitializeAsync(placement.IsTopmost);
            AppDiagnostics.Info("Main window initialization completed.");
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Main window initialization failed.", ex);
            MessageBox.Show(
                $"Desktop Calendar Overlay could not finish startup.\n\n{ex.Message}\n\nDiagnostic log:\n{AppDiagnostics.LogPath}",
                "Desktop Calendar Overlay",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e) =>
        _windowPlacementService.Save(this, _viewModel.IsTopmost);

    private void OnTitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed || IsInsideButton(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            return;
        }

        DragMove();
    }

    private static bool IsInsideButton(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ButtonBase)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private void OnOpenSettingsRequested(object? sender, EventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this,
            DataContext = _viewModel
        };

        settingsWindow.ShowDialog();
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
