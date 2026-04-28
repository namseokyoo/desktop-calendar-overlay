using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DesktopCalendarOverlay.Models;
using DesktopCalendarOverlay.Services;

namespace DesktopCalendarOverlay.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const string OverlaySettingsKey = "overlay-ui-settings";

    private readonly ICalendarService _calendarService;
    private readonly IGoogleCalendarIntegration? _googleIntegration;
    private readonly ISettingsStore _settingsStore;
    private DateOnly _visibleMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _isBusy;
    private bool _isDetailPanelExpanded = true;
    private bool _isTopmost;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private string _statusText = "Using mock calendar data. Connect Google Calendar from Settings.";
    private CalendarOverlaySettings _overlaySettings = new();

    public MainViewModel(
        ICalendarService calendarService,
        IGoogleCalendarIntegration? googleIntegration = null,
        ISettingsStore? settingsStore = null)
    {
        _calendarService = calendarService;
        _googleIntegration = googleIntegration;
        _settingsStore = settingsStore ?? new JsonSettingsStore();
        _overlaySettings = NormalizeSettings(_settingsStore.Read<CalendarOverlaySettings>(OverlaySettingsKey) ?? new CalendarOverlaySettings());
        ThemePaletteService.Apply(_overlaySettings.ThemeName);
        SelectDateCommand = new RelayCommand<DateOnly>(date =>
        {
            SelectedDate = date;
            IsDetailPanelExpanded = true;
        });
        PreviousMonthCommand = new RelayCommand(() => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(() => ChangeMonth(1));
        ToggleDetailPanelCommand = new RelayCommand(() => IsDetailPanelExpanded = !IsDetailPanelExpanded);
        OpenSettingsWindowCommand = new RelayCommand(() => OpenSettingsRequested?.Invoke(this, EventArgs.Empty));
        OpenCreateEventWindowCommand = new RelayCommand(() => OpenCreateEventRequested?.Invoke(this, EventArgs.Empty));
        ConnectGoogleCommand = new RelayCommand(() => _ = ConnectGoogleAsync());
        DisconnectGoogleCommand = new RelayCommand(() => _ = DisconnectGoogleAsync());
        RefreshCalendarCommand = new RelayCommand(() => _ = LoadCalendarAsync());
        ToggleLayerVisibilityCommand = new RelayCommand<CalendarLayer>(layer => _ = SaveLayerVisibilityAsync(layer));

        foreach (var header in BuildWeekdayHeaders())
        {
            WeekdayHeaders.Add(header);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? OpenSettingsRequested;

    public event EventHandler? OpenCreateEventRequested;

    public ObservableCollection<string> WeekdayHeaders { get; } = [];

    public ObservableCollection<CalendarLayer> CalendarLayers { get; } = [];

    public ObservableCollection<CalendarEvent> SelectedDayEvents { get; } = [];

    public ObservableCollection<CalendarDayViewModel> Days { get; } = [];

    public ICommand SelectDateCommand { get; }

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand ToggleDetailPanelCommand { get; }

    public ICommand OpenSettingsWindowCommand { get; }

    public ICommand OpenCreateEventWindowCommand { get; }

    public ICommand ConnectGoogleCommand { get; }

    public ICommand DisconnectGoogleCommand { get; }

    public ICommand RefreshCalendarCommand { get; }

    public ICommand ToggleLayerVisibilityCommand { get; }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetField(ref _selectedDate, value))
            {
                OnPropertyChanged(nameof(SelectedDateText));
                OnPropertyChanged(nameof(SelectedDayHeading));
                _ = LoadCalendarAsync();
            }
        }
    }

    public string SelectedDateText => SelectedDate.ToString("dddd, MMMM d", CultureInfo.CurrentCulture);

    public string SelectedDayHeading => SelectedDate == DateOnly.FromDateTime(DateTime.Today)
        ? "Today"
        : SelectedDate.ToString("dddd", CultureInfo.CurrentCulture);

    public string MonthTitle => _visibleMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public bool IsDetailPanelExpanded
    {
        get => _isDetailPanelExpanded;
        set
        {
            if (SetField(ref _isDetailPanelExpanded, value))
            {
                OnPropertyChanged(nameof(DetailPanelWidth));
                OnPropertyChanged(nameof(DetailPanelToggleText));
                OnPropertyChanged(nameof(DetailPanelToggleAccessibleText));
            }
        }
    }

    public double DetailPanelWidth => IsDetailPanelExpanded ? 340 : 64;

    public string DetailPanelToggleText => IsDetailPanelExpanded ? "Collapse details" : "Details";

    public string DetailPanelToggleAccessibleText => IsDetailPanelExpanded ? "Collapse detail panel" : "Expand detail panel";

    public bool IsTopmost
    {
        get => _isTopmost;
        set => SetField(ref _isTopmost, value);
    }

    public IReadOnlyList<string> EventDisplayModeOptions { get; } =
    [
        CalendarEventDisplayModes.TimeFirst,
        CalendarEventDisplayModes.EventFirst
    ];

    public IReadOnlyList<string> ThemeOptions { get; } =
    [
        CalendarThemeNames.AcrylicDark,
        CalendarThemeNames.IvoryEditorial,
        CalendarThemeNames.MidnightBlue
    ];

    public string SelectedEventDisplayMode
    {
        get => _overlaySettings.EventDisplayMode;
        set
        {
            var normalized = value == CalendarEventDisplayModes.EventFirst
                ? CalendarEventDisplayModes.EventFirst
                : CalendarEventDisplayModes.TimeFirst;
            if (_overlaySettings.EventDisplayMode == normalized)
            {
                return;
            }

            _overlaySettings = _overlaySettings with { EventDisplayMode = normalized };
            SaveOverlaySettings();
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEventFirstDisplay));
        }
    }

    public bool IsEventFirstDisplay => SelectedEventDisplayMode == CalendarEventDisplayModes.EventFirst;

    public double OverlayOpacity
    {
        get => _overlaySettings.OverlayOpacity;
        set
        {
            var normalized = Math.Clamp(Math.Round(value, 2), 0.35, 1.0);
            if (Math.Abs(_overlaySettings.OverlayOpacity - normalized) < 0.001)
            {
                return;
            }

            _overlaySettings = _overlaySettings with { OverlayOpacity = normalized };
            SaveOverlaySettings();
            OnPropertyChanged();
            OnPropertyChanged(nameof(OverlayOpacityPercentText));
        }
    }

    public string OverlayOpacityPercentText => $"{OverlayOpacity:P0}";

    public string SelectedThemeName
    {
        get => _overlaySettings.ThemeName;
        set
        {
            var normalized = ThemeOptions.Contains(value) ? value : CalendarThemeNames.AcrylicDark;
            if (_overlaySettings.ThemeName == normalized)
            {
                return;
            }

            _overlaySettings = _overlaySettings with { ThemeName = normalized };
            ThemePaletteService.Apply(normalized);
            SaveOverlaySettings();
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public string GoogleConnectionStatus
    {
        get
        {
            if (_googleIntegration is null)
            {
                return "Google integration unavailable";
            }

            if (!_googleIntegration.IsClientSecretAvailable)
            {
                return "OAuth client JSON not found";
            }

            return _googleIntegration.IsUsingGoogle
                ? "Connected to Google Calendar"
                : "OAuth client found; not connected yet";
        }
    }

    public string GoogleClientSecretPath => _googleIntegration?.ClientSecretPath ?? "Unavailable";

    public string GoogleTokenDirectory => _googleIntegration?.TokenDirectory ?? "Unavailable";

    public bool IsGoogleConnected => _googleIntegration?.IsUsingGoogle ?? false;

    public bool IsGoogleClientSecretAvailable => _googleIntegration?.IsClientSecretAvailable ?? false;

    public async Task InitializeAsync(bool isTopmost)
    {
        IsTopmost = isTopmost;
        await LoadCalendarAsync();
    }

    public async Task CreateCalendarEventAsync(CalendarEvent calendarEvent)
    {
        try
        {
            IsBusy = true;
            var created = await _calendarService.CreateEventAsync(calendarEvent);
            SelectedDate = DateOnly.FromDateTime(created.StartsAt.LocalDateTime);
            await LoadCalendarAsync();
            StatusText = $"Created event: {created.Title}";
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Calendar event creation failed.", ex);
            StatusText = $"Unable to create event: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ChangeMonth(int offset)
    {
        _visibleMonth = _visibleMonth.AddMonths(offset);
        OnPropertyChanged(nameof(MonthTitle));
        _ = LoadCalendarAsync();
    }

    private async Task ConnectGoogleAsync()
    {
        if (_googleIntegration is null)
        {
            StatusText = "Google integration is unavailable in this build.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusText = "Opening Google sign-in in your browser...";
            await _googleIntegration.ConnectAsync();
            NotifyGoogleStateChanged();
            await LoadCalendarAsync();
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Google Calendar connect failed.", ex);
            StatusText = $"Google connect failed: {ex.Message}";
            NotifyGoogleStateChanged();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DisconnectGoogleAsync()
    {
        if (_googleIntegration is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _googleIntegration.DisconnectAsync();
            NotifyGoogleStateChanged();
            await LoadCalendarAsync();
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Google Calendar disconnect failed.", ex);
            StatusText = $"Google disconnect failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveLayerVisibilityAsync(CalendarLayer layer)
    {
        if (_googleIntegration is null || !_googleIntegration.IsUsingGoogle)
        {
            return;
        }

        try
        {
            await _googleIntegration.SetLayerVisibilityAsync(layer.Id, layer.IsVisible);
            await LoadCalendarAsync();
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error($"Saving Google layer visibility failed for calendar '{layer.Id}'.", ex);
            StatusText = $"Unable to save layer visibility: {ex.Message}";
        }
    }

    private async Task LoadCalendarAsync()
    {
        try
        {
            IsBusy = true;
            var monthStart = _visibleMonth;
            var calendarStart = StartOfWeek(monthStart);
            var calendarEnd = calendarStart.AddDays(42);

            var layers = await _calendarService.GetLayersAsync();
            var monthEvents = await _calendarService.GetEventsAsync(calendarStart, calendarEnd);

            Replace(CalendarLayers, layers);
            Replace(Days, BuildDays(calendarStart, monthStart, SelectedDate, monthEvents));
            Replace(
                SelectedDayEvents,
                monthEvents
                    .Where(calendarEvent => DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime) == SelectedDate)
                    .OrderBy(calendarEvent => calendarEvent.IsAllDay ? 0 : 1)
                    .ThenBy(calendarEvent => calendarEvent.StartsAt)
                    .ToList());

            NotifyGoogleStateChanged();
            var sourceLabel = IsGoogleConnected ? "Google Calendar" : "mock calendar";
            StatusText = SelectedDayEvents.Count == 0
                ? $"No events for the selected day. Source: {sourceLabel}."
                : $"{SelectedDayEvents.Count} event{(SelectedDayEvents.Count == 1 ? string.Empty : "s")} selected. Source: {sourceLabel}.";
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Calendar preview load failed.", ex);
            StatusText = $"Unable to load calendar preview: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyGoogleStateChanged()
    {
        OnPropertyChanged(nameof(GoogleConnectionStatus));
        OnPropertyChanged(nameof(GoogleClientSecretPath));
        OnPropertyChanged(nameof(GoogleTokenDirectory));
        OnPropertyChanged(nameof(IsGoogleConnected));
        OnPropertyChanged(nameof(IsGoogleClientSecretAvailable));
    }

    private void SaveOverlaySettings()
    {
        try
        {
            _settingsStore.Write(OverlaySettingsKey, _overlaySettings);
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Unable to save overlay UI settings.", ex);
            StatusText = $"Unable to save overlay settings: {ex.Message}";
        }
    }

    private static CalendarOverlaySettings NormalizeSettings(CalendarOverlaySettings settings)
    {
        var displayMode = settings.EventDisplayMode == CalendarEventDisplayModes.EventFirst
            ? CalendarEventDisplayModes.EventFirst
            : CalendarEventDisplayModes.TimeFirst;
        var themeName = settings.ThemeName is CalendarThemeNames.IvoryEditorial or CalendarThemeNames.MidnightBlue
            ? settings.ThemeName
            : CalendarThemeNames.AcrylicDark;
        var opacity = Math.Clamp(settings.OverlayOpacity, 0.35, 1.0);
        return settings with
        {
            EventDisplayMode = displayMode,
            ThemeName = themeName,
            OverlayOpacity = opacity
        };
    }

    private static IReadOnlyList<CalendarDayViewModel> BuildDays(
        DateOnly calendarStart,
        DateOnly monthStart,
        DateOnly selectedDate,
        IReadOnlyList<CalendarEvent> events)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return Enumerable.Range(0, 42)
            .Select(offset => calendarStart.AddDays(offset))
            .Select(date => new CalendarDayViewModel
            {
                Date = date,
                DayNumber = date.Day.ToString(CultureInfo.CurrentCulture),
                IsInCurrentMonth = date.Month == monthStart.Month,
                IsToday = date == today,
                IsSelected = date == selectedDate,
                Events = events
                    .Where(calendarEvent => DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime) == date)
                    .OrderBy(calendarEvent => calendarEvent.IsAllDay ? 0 : 1)
                    .ThenBy(calendarEvent => calendarEvent.StartsAt)
                    .ToList()
            })
            .ToList();
    }

    private static IReadOnlyList<string> BuildWeekdayHeaders()
    {
        var culture = CultureInfo.CurrentCulture;
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        return Enumerable.Range(0, 7)
            .Select(offset => (DayOfWeek)(((int)firstDayOfWeek + offset) % 7))
            .Select(dayOfWeek => culture.DateTimeFormat.AbbreviatedDayNames[(int)dayOfWeek])
            .ToList();
    }

    private static DateOnly StartOfWeek(DateOnly monthStart)
    {
        var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var delta = ((7 + (int)monthStart.DayOfWeek - (int)firstDayOfWeek) % 7);
        return monthStart.AddDays(-delta);
    }

    private static void Replace<T>(ObservableCollection<T> collection, IEnumerable<T> values)
    {
        collection.Clear();
        foreach (var value in values)
        {
            collection.Add(value);
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
