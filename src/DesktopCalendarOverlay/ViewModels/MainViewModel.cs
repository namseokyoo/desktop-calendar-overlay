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
    private readonly ICalendarService _calendarService;
    private DateOnly _visibleMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _isDetailPanelExpanded = true;
    private bool _isTopmost;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private string _statusText = "Using mock calendar data. Connect Google Calendar later from Settings.";

    public MainViewModel(ICalendarService calendarService)
    {
        _calendarService = calendarService;
        SelectDateCommand = new RelayCommand<DateOnly>(date =>
        {
            SelectedDate = date;
            IsDetailPanelExpanded = true;
        });
        PreviousMonthCommand = new RelayCommand(() => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(() => ChangeMonth(1));
        ToggleDetailPanelCommand = new RelayCommand(() => IsDetailPanelExpanded = !IsDetailPanelExpanded);
        OpenSettingsWindowCommand = new RelayCommand(() => OpenSettingsRequested?.Invoke(this, EventArgs.Empty));

        foreach (var header in BuildWeekdayHeaders())
        {
            WeekdayHeaders.Add(header);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? OpenSettingsRequested;

    public ObservableCollection<string> WeekdayHeaders { get; } = [];

    public ObservableCollection<CalendarLayer> CalendarLayers { get; } = [];

    public ObservableCollection<CalendarEvent> SelectedDayEvents { get; } = [];

    public ObservableCollection<CalendarDayViewModel> Days { get; } = [];

    public ICommand SelectDateCommand { get; }

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand ToggleDetailPanelCommand { get; }

    public ICommand OpenSettingsWindowCommand { get; }

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

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public async Task InitializeAsync(bool isTopmost)
    {
        IsTopmost = isTopmost;
        await LoadCalendarAsync();
    }

    private void ChangeMonth(int offset)
    {
        _visibleMonth = _visibleMonth.AddMonths(offset);
        OnPropertyChanged(nameof(MonthTitle));
        _ = LoadCalendarAsync();
    }

    private async Task LoadCalendarAsync()
    {
        try
        {
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

            StatusText = SelectedDayEvents.Count == 0
                ? "No events for the selected day. Mock data only; Google sync is intentionally not wired yet."
                : $"{SelectedDayEvents.Count} mock event{(SelectedDayEvents.Count == 1 ? string.Empty : "s")} selected. Google Calendar auth belongs in the separate Settings window later.";
        }
        catch (Exception ex)
        {
            StatusText = $"Unable to load calendar preview: {ex.Message}";
        }
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
