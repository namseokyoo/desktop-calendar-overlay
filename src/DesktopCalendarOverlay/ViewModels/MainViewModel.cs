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
    private readonly DateOnly _visibleMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private readonly RelayCommand _toggleSettingsCommand;
    private bool _isSettingsOpen;
    private bool _isTopmost;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private string _statusText = "Using mock calendar data. Connect Google Calendar later from Settings.";

    public MainViewModel(ICalendarService calendarService)
    {
        _calendarService = calendarService;
        _toggleSettingsCommand = new RelayCommand(() => IsSettingsOpen = !IsSettingsOpen);
        SelectDateCommand = new RelayCommand<DateOnly>(date => SelectedDate = date);
        ToggleSettingsCommand = _toggleSettingsCommand;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CalendarLayer> CalendarLayers { get; } = [];

    public ObservableCollection<CalendarEvent> SelectedDayEvents { get; } = [];

    public ObservableCollection<CalendarDayViewModel> Days { get; } = [];

    public ICommand SelectDateCommand { get; }

    public ICommand ToggleSettingsCommand { get; }

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

    public string SettingsButtonText => IsSettingsOpen ? "Hide settings" : "Settings";

    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set
        {
            if (SetField(ref _isSettingsOpen, value))
            {
                OnPropertyChanged(nameof(SettingsButtonText));
            }
        }
    }

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
                    .OrderBy(calendarEvent => calendarEvent.StartsAt)
                    .ToList());

            StatusText = SelectedDayEvents.Count == 0
                ? "No events for the selected day. Mock data only; Google sync is intentionally not wired yet."
                : $"{SelectedDayEvents.Count} mock event{(SelectedDayEvents.Count == 1 ? string.Empty : "s")} selected. Google Calendar auth belongs in Settings later.";
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
                DayName = date.ToString("ddd", CultureInfo.CurrentCulture),
                DayNumber = date.Day.ToString(CultureInfo.CurrentCulture),
                IsInCurrentMonth = date.Month == monthStart.Month,
                IsToday = date == today,
                IsSelected = date == selectedDate,
                Events = events
                    .Where(calendarEvent => DateOnly.FromDateTime(calendarEvent.StartsAt.LocalDateTime) == date)
                    .OrderBy(calendarEvent => calendarEvent.StartsAt)
                    .ToList()
            })
            .ToList();
    }

    private DateOnly StartOfWeek(DateOnly monthStart)
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
