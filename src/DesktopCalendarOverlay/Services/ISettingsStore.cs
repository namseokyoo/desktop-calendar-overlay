namespace DesktopCalendarOverlay.Services;

public interface ISettingsStore
{
    T? Read<T>(string key);

    void Write<T>(string key, T value);

    void Delete(string key);
}
