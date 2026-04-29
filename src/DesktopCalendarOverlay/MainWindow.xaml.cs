using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Forms = System.Windows.Forms;
using DesktopCalendarOverlay.Services;
using DesktopCalendarOverlay.ViewModels;

namespace DesktopCalendarOverlay;

public partial class MainWindow : Window
{
    private readonly IWindowPlacementService _windowPlacementService;
    private readonly MainViewModel _viewModel;
    private readonly Forms.NotifyIcon _trayIcon;
    private bool _isExitRequested;

    public MainWindow()
    {
        InitializeComponent();

        var settingsStore = new JsonSettingsStore();
        _windowPlacementService = new WindowPlacementService(settingsStore);
        var googleCalendarService = new GoogleCalendarService(settingsStore);
        var calendarService = new CalendarServiceRouter(googleCalendarService, new MockCalendarService());
        _viewModel = new MainViewModel(calendarService, calendarService, settingsStore);
        _viewModel.OpenSettingsRequested += OnOpenSettingsRequested;
        _viewModel.OpenCreateEventRequested += OnOpenCreateEventRequested;
        _viewModel.OpenEditEventRequested += OnOpenEditEventRequested;
        _viewModel.DeleteEventRequested += OnDeleteEventRequested;
        _viewModel.PositionLockChanged += OnPositionLockChanged;
        DataContext = _viewModel;

        _trayIcon = CreateTrayIcon();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            AppDiagnostics.Info("Main window loaded; applying placement and loading calendar preview.");
            var placement = _windowPlacementService.Load();
            _windowPlacementService.Apply(this, placement);
            await _viewModel.InitializeAsync(placement.IsPositionLocked);
            ApplyPositionLock();
            AppDiagnostics.Info("Main window initialization completed.");
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Main window initialization failed.", ex);
            System.Windows.MessageBox.Show(
                $"Desktop Calendar Overlay could not finish startup.\n\n{ex.Message}\n\nDiagnostic log:\n{AppDiagnostics.LogPath}",
                "Desktop Calendar Overlay",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        _windowPlacementService.Save(this, _viewModel.IsPositionLocked);
        if (!_isExitRequested && _trayIcon.Visible)
        {
            e.Cancel = true;
            Hide();
            _trayIcon.ShowBalloonTip(1500, "Desktop Calendar Overlay", "Overlay hidden. Use the tray icon to show it again or exit.", Forms.ToolTipIcon.Info);
        }
    }

    private void OnTitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed || IsInsideButton(e.OriginalSource as DependencyObject) || _viewModel.IsPositionLocked)
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
            if (source is System.Windows.Controls.Primitives.ButtonBase)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private void OnOpenSettingsRequested(object? sender, EventArgs e) => ShowSettingsWindow();

    private void ShowSettingsWindow()
    {
        ShowOverlay();
        var settingsWindow = new SettingsWindow
        {
            Owner = this,
            DataContext = _viewModel
        };

        settingsWindow.ShowDialog();
    }

    private async void OnOpenCreateEventRequested(object? sender, EventArgs e)
    {
        var createEventWindow = new CreateEventWindow(_viewModel.SelectedDate, _viewModel.CalendarLayers)
        {
            Owner = this
        };

        if (createEventWindow.ShowDialog() == true && createEventWindow.CreatedEvent is not null)
        {
            await _viewModel.CreateCalendarEventAsync(createEventWindow.CreatedEvent);
        }
    }

    private async void OnOpenEditEventRequested(object? sender, DesktopCalendarOverlay.Models.CalendarEvent calendarEvent)
    {
        var editEventWindow = new CreateEventWindow(_viewModel.SelectedDate, _viewModel.CalendarLayers, calendarEvent)
        {
            Owner = this
        };

        if (editEventWindow.ShowDialog() == true && editEventWindow.CreatedEvent is not null)
        {
            await _viewModel.UpdateCalendarEventAsync(editEventWindow.CreatedEvent);
        }
    }

    private async void OnDeleteEventRequested(object? sender, DesktopCalendarOverlay.Models.CalendarEvent calendarEvent)
    {
        var result = System.Windows.MessageBox.Show(
            $"Delete '{calendarEvent.Title}'?",
            "Delete event",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            await _viewModel.DeleteCalendarEventAsync(calendarEvent);
        }
    }

    private void OnPositionLockChanged(object? sender, EventArgs e)
    {
        ApplyPositionLock();
        _windowPlacementService.Save(this, _viewModel.IsPositionLocked);
    }

    private void ApplyPositionLock()
    {
        ResizeMode = _viewModel.IsPositionLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private Forms.NotifyIcon CreateTrayIcon()
    {
        var showHideItem = new Forms.ToolStripMenuItem("Show/Hide", null, (_, _) => ToggleOverlayVisibility());
        var settingsItem = new Forms.ToolStripMenuItem("Settings", null, (_, _) => ShowSettingsWindow());
        var refreshItem = new Forms.ToolStripMenuItem("Refresh", null, (_, _) =>
        {
            ShowOverlay();
            if (_viewModel.RefreshCalendarCommand.CanExecute(null))
            {
                _viewModel.RefreshCalendarCommand.Execute(null);
            }
        });
        var exitItem = new Forms.ToolStripMenuItem("Exit", null, (_, _) => ExitApplication());
        var menu = new Forms.ContextMenuStrip();
        menu.Items.AddRange([showHideItem, settingsItem, refreshItem, new Forms.ToolStripSeparator(), exitItem]);

        var icon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? Forms.Application.ExecutablePath) ?? System.Drawing.SystemIcons.Application,
            Text = "Desktop Calendar Overlay",
            ContextMenuStrip = menu,
            Visible = true
        };
        icon.DoubleClick += (_, _) => ToggleOverlayVisibility();
        return icon;
    }

    private void ToggleOverlayVisibility()
    {
        if (IsVisible && WindowState != WindowState.Minimized)
        {
            Hide();
            return;
        }

        ShowOverlay();
    }

    private void ShowOverlay()
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    private void ExitApplication()
    {
        _isExitRequested = true;
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Close();
    }
}
