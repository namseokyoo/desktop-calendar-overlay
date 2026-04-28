using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopCalendarOverlay.Models;
using DesktopCalendarOverlay.Services;

namespace DesktopCalendarOverlay.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ICalendarService _calendarService;
    private bool _isTopmost;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private string _statusText = "Using mock calendar data. Connect Google Calendar later from Settings.";

    public MainViewModel(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CalendarLayer> CalendarLayers { get; } = [];

    public ObservableCollection<CalendarEvent> Events { get; } = [];

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetField(ref _selectedDate, value))
            {
                OnPropertyChanged(nameof(SelectedDateText));
                _ = LoadEventsAsync();
            }
        }
    }

    public string SelectedDateText => SelectedDate.ToString("dddd, MMMM d");

    public string MonthTitle => DateTime.Today.ToString("MMMM yyyy");

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
        CalendarLayers.Clear();
        foreach (var layer in await _calendarService.GetLayersAsync())
        {
            CalendarLayers.Add(layer);
        }

        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        try
        {
            Events.Clear();
            foreach (var calendarEvent in await _calendarService.GetEventsAsync(SelectedDate, SelectedDate.AddDays(1)))
            {
                Events.Add(calendarEvent);
            }

            StatusText = Events.Count == 0
                ? "No events for the selected day."
                : $"{Events.Count} event{(Events.Count == 1 ? string.Empty : "s")} for the selected day.";
        }
        catch (Exception ex)
        {
            StatusText = $"Unable to load calendar preview: {ex.Message}";
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
