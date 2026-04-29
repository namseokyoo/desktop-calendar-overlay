using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DesktopCalendarOverlay.Models;
using DesktopCalendarOverlay.Services;

namespace DesktopCalendarOverlay.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const string OverlaySettingsKey = "overlay-ui-settings";
    private const string LayerColorOverridesKey = "calendar-layer-color-overrides";
    private const string LayerVisibilityOverridesKey = "google-calendar-layer-visibility";

    private readonly ICalendarService _calendarService;
    private readonly IGoogleCalendarIntegration? _googleIntegration;
    private readonly ISettingsStore _settingsStore;
    private DateOnly _visibleMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _isBusy;
    private bool _isDetailPanelExpanded = true;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private string _statusText = "Using mock calendar data. Connect Google Calendar from Settings.";
    private CalendarOverlaySettings _overlaySettings = new();
    private readonly StartupRegistrationService _startupRegistrationService = new();
    private IReadOnlyList<CalendarEvent> _loadedMonthEvents = [];
    private IReadOnlyDictionary<DateOnly, IReadOnlyList<CalendarEvent>> _loadedEventsByDate = new Dictionary<DateOnly, IReadOnlyList<CalendarEvent>>();

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
        TogglePositionLockCommand = new RelayCommand(() => IsPositionLocked = !IsPositionLocked);
        OpenSettingsWindowCommand = new RelayCommand(() => OpenSettingsRequested?.Invoke(this, EventArgs.Empty));
        OpenCreateEventWindowCommand = new RelayCommand(() => OpenCreateEventRequested?.Invoke(this, EventArgs.Empty));
        OpenEditEventWindowCommand = new RelayCommand<CalendarEvent>(calendarEvent => OpenEditEventRequested?.Invoke(this, calendarEvent));
        DeleteEventCommand = new RelayCommand<CalendarEvent>(calendarEvent => DeleteEventRequested?.Invoke(this, calendarEvent));
        ConnectGoogleCommand = new RelayCommand(() => _ = ConnectGoogleAsync());
        DisconnectGoogleCommand = new RelayCommand(() => _ = DisconnectGoogleAsync());
        RefreshCalendarCommand = new RelayCommand(() => _ = LoadCalendarAsync());
        ToggleLayerVisibilityCommand = new RelayCommand<CalendarLayer>(layer => _ = SaveLayerVisibilityAsync(layer));
        UpdateLayerColorCommand = new RelayCommand<CalendarLayer>(layer => _ = SaveLayerColorAsync(layer));

        foreach (var header in BuildWeekdayHeaders())
        {
            WeekdayHeaders.Add(header);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? OpenSettingsRequested;

    public event EventHandler? OpenCreateEventRequested;

    public event EventHandler<CalendarEvent>? OpenEditEventRequested;

    public event EventHandler<CalendarEvent>? DeleteEventRequested;

    public event EventHandler? PositionLockChanged;

    public ObservableCollection<string> WeekdayHeaders { get; } = [];

    public ObservableCollection<CalendarLayer> CalendarLayers { get; } = [];

    public ObservableCollection<CalendarEvent> SelectedDayEvents { get; } = [];

    public ObservableCollection<CalendarDayViewModel> Days { get; } = [];

    public ICommand SelectDateCommand { get; }

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand ToggleDetailPanelCommand { get; }

    public ICommand TogglePositionLockCommand { get; }

    public ICommand OpenSettingsWindowCommand { get; }

    public ICommand OpenCreateEventWindowCommand { get; }

    public ICommand OpenEditEventWindowCommand { get; }

    public ICommand DeleteEventCommand { get; }

    public ICommand ConnectGoogleCommand { get; }

    public ICommand DisconnectGoogleCommand { get; }

    public ICommand RefreshCalendarCommand { get; }

    public ICommand ToggleLayerVisibilityCommand { get; }

    public ICommand UpdateLayerColorCommand { get; }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetField(ref _selectedDate, value))
            {
                OnPropertyChanged(nameof(SelectedDateText));
                OnPropertyChanged(nameof(SelectedDayHeading));
                RefreshSelectedDateFromCache();
            }
        }
    }

    public string SelectedDateText => SelectedDate.ToString("dddd, MMMM d", CultureInfo.CurrentCulture);

    public string SelectedDayHeading => SelectedDate == DateOnly.FromDateTime(DateTime.Today)
        ? "Today"
        : SelectedDate.ToString("dddd", CultureInfo.CurrentCulture);

    public string MonthTitle => _visibleMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

    public string VersionLabel => "v0.6.1-performance-cache";

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

    public bool IsPositionLocked
    {
        get => _overlaySettings.IsPositionLocked;
        set
        {
            if (_overlaySettings.IsPositionLocked == value)
            {
                return;
            }

            _overlaySettings = _overlaySettings with { IsPositionLocked = value };
            SaveOverlaySettings();
            OnPropertyChanged();
            OnPropertyChanged(nameof(PositionLockText));
            PositionLockChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string PositionLockText => IsPositionLocked ? "Position locked" : "Lock position";

    public bool StartWithWindows
    {
        get => _overlaySettings.StartWithWindows && _startupRegistrationService.IsEnabled;
        set
        {
            if (!_startupRegistrationService.IsSupported)
            {
                StatusText = "Windows startup can only be changed when running on Windows.";
                OnPropertyChanged();
                return;
            }

            if (_overlaySettings.StartWithWindows == value && _startupRegistrationService.IsEnabled == value)
            {
                return;
            }

            try
            {
                _startupRegistrationService.IsEnabled = value;
                _overlaySettings = _overlaySettings with { StartWithWindows = value };
                SaveOverlaySettings();
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartWithWindowsStatus));
                StatusText = value
                    ? "Windows startup enabled for Desktop Calendar Overlay."
                    : "Windows startup disabled for Desktop Calendar Overlay.";
            }
            catch (Exception ex)
            {
                StatusText = FriendlyErrorMessage("Unable to update Windows startup", ex);
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartWithWindowsStatus));
            }
        }
    }

    public string StartWithWindowsStatus => _startupRegistrationService.IsSupported
        ? "Launch Desktop Calendar Overlay after Windows sign-in."
        : "Windows startup is available only when running on Windows.";

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

    public IReadOnlyList<string> LayerColorPalette { get; } =
    [
        "#7DD3FC",
        "#38BDF8",
        "#A78BFA",
        "#F9A8D4",
        "#FB7185",
        "#F97316",
        "#FACC15",
        "#86EFAC",
        "#34D399",
        "#F8FAFC"
    ];

    public double EventListFontSize
    {
        get => _overlaySettings.EventListFontSize;
        set
        {
            var normalized = Math.Clamp(Math.Round(value, 1), 9.0, 16.0);
            if (Math.Abs(_overlaySettings.EventListFontSize - normalized) < 0.001)
            {
                return;
            }

            _overlaySettings = _overlaySettings with { EventListFontSize = normalized };
            SaveOverlaySettings();
            OnPropertyChanged();
            OnPropertyChanged(nameof(EventListFontSizeText));
        }
    }

    public string EventListFontSizeText => $"{EventListFontSize:0.#} pt";

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

    public async Task InitializeAsync(bool isPositionLocked)
    {
        IsPositionLocked = isPositionLocked;
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
            StatusText = FriendlyErrorMessage("Unable to create event", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task UpdateCalendarEventAsync(CalendarEvent calendarEvent)
    {
        try
        {
            IsBusy = true;
            var updated = await _calendarService.UpdateEventAsync(calendarEvent);
            SelectedDate = DateOnly.FromDateTime(updated.StartsAt.LocalDateTime);
            await LoadCalendarAsync();
            StatusText = $"Updated event: {updated.Title}";
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Calendar event update failed.", ex);
            StatusText = FriendlyErrorMessage("Unable to update event", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeleteCalendarEventAsync(CalendarEvent calendarEvent)
    {
        try
        {
            IsBusy = true;
            await _calendarService.DeleteEventAsync(calendarEvent);
            await LoadCalendarAsync();
            StatusText = $"Deleted event: {calendarEvent.Title}";
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Calendar event delete failed.", ex);
            StatusText = FriendlyErrorMessage("Unable to delete event", ex);
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
            StatusText = FriendlyErrorMessage("Google connect failed", ex);
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
            StatusText = FriendlyErrorMessage("Google disconnect failed", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveLayerVisibilityAsync(CalendarLayer layer)
    {
        try
        {
            var overrides = LoadLayerVisibilityOverrides();
            overrides[layer.Id] = layer.IsVisible;
            _settingsStore.Write(LayerVisibilityOverridesKey, overrides);

            if (_googleIntegration is not null)
            {
                await _googleIntegration.SetLayerVisibilityAsync(layer.Id, layer.IsVisible);
            }

            await LoadCalendarAsync();
            StatusText = layer.IsVisible
                ? $"Layer shown: {layer.Name}"
                : $"Layer hidden: {layer.Name}";
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error($"Saving layer visibility failed for calendar '{layer.Id}'.", ex);
            StatusText = FriendlyErrorMessage("Unable to save layer visibility", ex);
        }
    }

    private Task SaveLayerColorAsync(CalendarLayer layer)
    {
        try
        {
            var overrides = LoadLayerColorOverrides();
            overrides[layer.Id] = layer.ColorHex;
            _settingsStore.Write(LayerColorOverridesKey, overrides);
            ApplyLayerColorToLoadedEvents(layer.Id, layer.ColorHex);
            StatusText = $"Updated layer color: {layer.Name}";
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error($"Saving calendar layer color failed for calendar '{layer.Id}'.", ex);
            StatusText = FriendlyErrorMessage("Unable to save layer color", ex);
        }

        return Task.CompletedTask;
    }

    private async Task LoadCalendarAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            IsBusy = true;
            var monthStart = _visibleMonth;
            var calendarStart = StartOfWeek(monthStart);
            var calendarEnd = calendarStart.AddDays(42);

            var layers = ApplyLayerVisibilityOverrides(ApplyLayerColorOverrides(await _calendarService.GetLayersAsync()));
            var visibleLayerIds = layers
                .Where(layer => layer.IsVisible)
                .Select(layer => layer.Id)
                .ToHashSet(StringComparer.Ordinal);
            var monthEvents = ApplyLayerColors(await _calendarService.GetEventsAsync(calendarStart, calendarEnd), layers)
                .Where(calendarEvent => visibleLayerIds.Contains(calendarEvent.CalendarLayerId))
                .ToList();

            Replace(CalendarLayers, layers);
            UpdateLoadedEventCache(monthEvents);
            Replace(Days, BuildDays(calendarStart, monthStart, SelectedDate, _loadedEventsByDate));
            Replace(SelectedDayEvents, EventsForDate(SelectedDate));

            NotifyGoogleStateChanged();
            stopwatch.Stop();
            AppDiagnostics.Info($"Calendar refresh loaded {monthEvents.Count} event(s) in {stopwatch.ElapsedMilliseconds} ms.");
            SetSelectedDayStatus($" refreshed in {stopwatch.ElapsedMilliseconds} ms");
        }
        catch (Exception ex)
        {
            AppDiagnostics.Error("Calendar preview load failed.", ex);
            StatusText = FriendlyErrorMessage("Unable to refresh calendar", ex);
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
            StatusText = FriendlyErrorMessage("Unable to save overlay settings", ex);
        }
    }

    private void ApplyLayerColorToLoadedEvents(string layerId, string colorHex)
    {
        var updatedEvents = _loadedMonthEvents
            .Select(calendarEvent => calendarEvent.CalendarLayerId == layerId
                ? calendarEvent with { LayerColorHex = colorHex }
                : calendarEvent)
            .ToList();
        UpdateLoadedEventCache(updatedEvents);
        Replace(Days, BuildDays(StartOfWeek(_visibleMonth), _visibleMonth, SelectedDate, _loadedEventsByDate));
        Replace(SelectedDayEvents, EventsForDate(SelectedDate));
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
        var eventListFontSize = Math.Clamp(settings.EventListFontSize <= 0 ? 10.0 : settings.EventListFontSize, 9.0, 16.0);
        return settings with
        {
            EventDisplayMode = displayMode,
            ThemeName = themeName,
            OverlayOpacity = opacity,
            EventListFontSize = eventListFontSize
        };
    }

    private IReadOnlyList<CalendarLayer> ApplyLayerVisibilityOverrides(IReadOnlyList<CalendarLayer> layers)
    {
        var overrides = LoadLayerVisibilityOverrides();
        foreach (var layer in layers)
        {
            if (overrides.TryGetValue(layer.Id, out var isVisible))
            {
                layer.IsVisible = isVisible;
            }
        }

        return layers;
    }

    private IReadOnlyList<CalendarLayer> ApplyLayerColorOverrides(IReadOnlyList<CalendarLayer> layers)
    {
        var overrides = LoadLayerColorOverrides();
        foreach (var layer in layers)
        {
            if (overrides.TryGetValue(layer.Id, out var colorHex) && !string.IsNullOrWhiteSpace(colorHex))
            {
                layer.ColorHex = colorHex;
            }
        }

        return layers;
    }

    private IReadOnlyList<CalendarEvent> ApplyLayerColors(IReadOnlyList<CalendarEvent> events, IReadOnlyList<CalendarLayer> layers)
    {
        var colorByLayerId = layers.ToDictionary(layer => layer.Id, layer => layer.ColorHex, StringComparer.Ordinal);
        return events
            .Select(calendarEvent => calendarEvent with
            {
                LayerColorHex = colorByLayerId.TryGetValue(calendarEvent.CalendarLayerId, out var colorHex)
                    ? colorHex
                    : calendarEvent.LayerColorHex
            })
            .ToList();
    }

    private Dictionary<string, string> LoadLayerColorOverrides() =>
        _settingsStore.Read<Dictionary<string, string>>(LayerColorOverridesKey) ?? [];

    private Dictionary<string, bool> LoadLayerVisibilityOverrides() =>
        _settingsStore.Read<Dictionary<string, bool>>(LayerVisibilityOverridesKey) ?? [];

    private static string FriendlyErrorMessage(string action, Exception exception)
    {
        var reason = exception switch
        {
            FileNotFoundException => "required local file is missing",
            UnauthorizedAccessException => "permission was denied",
            HttpRequestException => "network request failed",
            InvalidOperationException => exception.Message,
            _ when exception.GetType().Name.Contains("Token", StringComparison.OrdinalIgnoreCase) => "Google authorization token could not be used",
            _ when exception.GetType().Name.Contains("Google", StringComparison.OrdinalIgnoreCase) => "Google Calendar request failed",
            _ => exception.Message
        };

        return $"{action}: {reason}. See diagnostic log: {AppDiagnostics.LogPath}";
    }

    private void RefreshSelectedDateFromCache()
    {
        var stopwatch = Stopwatch.StartNew();
        Replace(Days, BuildDays(StartOfWeek(_visibleMonth), _visibleMonth, SelectedDate, _loadedEventsByDate));
        Replace(SelectedDayEvents, EventsForDate(SelectedDate));
        stopwatch.Stop();
        AppDiagnostics.Info($"Calendar date selection updated from cache in {stopwatch.ElapsedMilliseconds} ms for {SelectedDate:yyyy-MM-dd}.");
        SetSelectedDayStatus($" selected from cache in {stopwatch.ElapsedMilliseconds} ms");
    }

    private void UpdateLoadedEventCache(IReadOnlyList<CalendarEvent> events)
    {
        _loadedMonthEvents = events;
        _loadedEventsByDate = events
            .GroupBy(calendarEvent => DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime))
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<CalendarEvent>)group
                    .OrderBy(calendarEvent => calendarEvent.IsAllDay ? 0 : 1)
                    .ThenBy(calendarEvent => calendarEvent.StartsAt)
                    .ToList());
    }

    private IReadOnlyList<CalendarEvent> EventsForDate(DateOnly date) =>
        _loadedEventsByDate.TryGetValue(date, out var events) ? events : [];

    private void SetSelectedDayStatus(string timingSuffix)
    {
        var sourceLabel = IsGoogleConnected ? "Google Calendar" : "mock calendar";
        StatusText = SelectedDayEvents.Count == 0
            ? $"No events for the selected day. Source: {sourceLabel};{timingSuffix}."
            : $"{SelectedDayEvents.Count} event{(SelectedDayEvents.Count == 1 ? string.Empty : "s")} selected. Source: {sourceLabel};{timingSuffix}.";
    }

    private static IReadOnlyList<CalendarDayViewModel> BuildDays(
        DateOnly calendarStart,
        DateOnly monthStart,
        DateOnly selectedDate,
        IReadOnlyDictionary<DateOnly, IReadOnlyList<CalendarEvent>> eventsByDate)
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
                Events = eventsByDate.TryGetValue(date, out var events) ? events : []
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
