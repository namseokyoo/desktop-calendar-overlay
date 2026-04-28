using System.IO;
using System.Text.Json;

namespace DesktopCalendarOverlay.Services;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly string _settingsDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public JsonSettingsStore()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopCalendarOverlay");
    }

    public T? Read<T>(string key)
    {
        var path = GetPath(key);
        if (!File.Exists(path))
        {
            return default;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public void Write<T>(string key, T value)
    {
        Directory.CreateDirectory(_settingsDirectory);
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        File.WriteAllText(GetPath(key), json);
    }

    public void Delete(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetPath(string key)
    {
        var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return Path.Combine(_settingsDirectory, $"{safeKey}.json");
    }
}
