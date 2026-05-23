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

    public JsonSettingsStore(string? settingsDirectory = null)
    {
        _settingsDirectory = settingsDirectory ?? Path.Combine(
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

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            AppDiagnostics.Error($"Unable to read settings key '{key}'. Falling back to defaults.", ex);
            return default;
        }
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
