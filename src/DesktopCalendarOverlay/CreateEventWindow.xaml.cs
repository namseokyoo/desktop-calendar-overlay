using System.Globalization;
using System.Windows;
using DesktopCalendarOverlay.Models;

namespace DesktopCalendarOverlay;

public partial class CreateEventWindow : Window
{
    public CreateEventWindow(DateOnly selectedDate, IEnumerable<CalendarLayer> layers)
    {
        InitializeComponent();
        EventDatePicker.SelectedDate = selectedDate.ToDateTime(TimeOnly.MinValue);
        StartTimeBox.Text = "09:00";
        EndTimeBox.Text = "10:00";
        CalendarLayerBox.ItemsSource = layers.Where(layer => layer.IsVisible).ToList();
        CalendarLayerBox.SelectedIndex = CalendarLayerBox.Items.Count > 0 ? 0 : -1;
    }

    public CalendarEvent? CreatedEvent { get; private set; }

    private void OnCreateClick(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        if (CalendarLayerBox.SelectedItem is not CalendarLayer layer)
        {
            ErrorText.Text = "Select a calendar layer first.";
            return;
        }

        var title = TitleBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorText.Text = "Title is required.";
            return;
        }

        if (EventDatePicker.SelectedDate is not DateTime date)
        {
            ErrorText.Text = "Date is required.";
            return;
        }

        var dateOnly = DateOnly.FromDateTime(date);
        DateTimeOffset startsAt;
        DateTimeOffset endsAt;
        if (AllDayCheckBox.IsChecked == true)
        {
            var startDate = dateOnly.ToDateTime(TimeOnly.MinValue);
            startsAt = new DateTimeOffset(startDate, TimeZoneInfo.Local.GetUtcOffset(startDate));
            endsAt = startsAt.AddDays(1);
        }
        else
        {
            if (!TryReadTime(StartTimeBox.Text, out var startTime) || !TryReadTime(EndTimeBox.Text, out var endTime))
            {
                ErrorText.Text = "Use HH:mm time format, for example 09:30.";
                return;
            }

            var startDateTime = dateOnly.ToDateTime(startTime);
            var endDateTime = dateOnly.ToDateTime(endTime);
            if (endDateTime <= startDateTime)
            {
                ErrorText.Text = "End time must be later than start time.";
                return;
            }

            startsAt = new DateTimeOffset(startDateTime, TimeZoneInfo.Local.GetUtcOffset(startDateTime));
            endsAt = new DateTimeOffset(endDateTime, TimeZoneInfo.Local.GetUtcOffset(endDateTime));
        }

        CreatedEvent = new CalendarEvent(
            string.Empty,
            layer.Id,
            title,
            startsAt,
            endsAt,
            AllDayCheckBox.IsChecked == true,
            string.IsNullOrWhiteSpace(LocationBox.Text) ? null : LocationBox.Text.Trim());
        DialogResult = true;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => DialogResult = false;

    private static bool TryReadTime(string raw, out TimeOnly time) =>
        TimeOnly.TryParseExact(raw.Trim(), ["H:mm", "HH:mm"], CultureInfo.CurrentCulture, DateTimeStyles.None, out time) ||
        TimeOnly.TryParse(raw.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out time);
}
