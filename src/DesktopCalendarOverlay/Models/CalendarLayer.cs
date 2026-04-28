using System.ComponentModel;

namespace DesktopCalendarOverlay.Models;

public sealed class CalendarLayer : INotifyPropertyChanged
{
    private bool _isVisible;
    private string _colorHex;

    public CalendarLayer(string id, string name, string colorHex, bool isVisible, bool isPrimary = false)
    {
        Id = id;
        Name = name;
        _colorHex = NormalizeColor(colorHex);
        _isVisible = isVisible;
        IsPrimary = isPrimary;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; }

    public string Name { get; }

    public string ColorHex
    {
        get => _colorHex;
        set
        {
            var normalized = NormalizeColor(value);
            if (StringComparer.OrdinalIgnoreCase.Equals(_colorHex, normalized))
            {
                return;
            }

            _colorHex = normalized;
            OnPropertyChanged(nameof(ColorHex));
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value)
            {
                return;
            }

            _isVisible = value;
            OnPropertyChanged(nameof(IsVisible));
        }
    }

    public bool IsPrimary { get; }

    private static string NormalizeColor(string? color) =>
        string.IsNullOrWhiteSpace(color) ? "#7DD3FC" : color;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
