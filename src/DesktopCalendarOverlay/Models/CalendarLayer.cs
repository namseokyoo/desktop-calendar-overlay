using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopCalendarOverlay.Models;

public sealed class CalendarLayer : INotifyPropertyChanged
{
    private bool _isVisible;

    public CalendarLayer(string id, string name, string colorHex, bool isVisible, bool isPrimary = false)
    {
        Id = id;
        Name = name;
        ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#7DD3FC" : colorHex;
        _isVisible = isVisible;
        IsPrimary = isPrimary;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; }

    public string Name { get; }

    public string ColorHex { get; }

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
        }
    }

    public bool IsPrimary { get; }
}
