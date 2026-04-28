using System.IO;

namespace DesktopCalendarOverlay.Services;

public static class AppDiagnostics
{
    private static readonly object SyncRoot = new();

    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopCalendarOverlay",
        "logs");

    public static string LogPath { get; } = Path.Combine(LogDirectory, "startup.log");

    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message, Exception exception) =>
        Write("ERROR", $"{message}{Environment.NewLine}{exception}");

    private static void Write(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var line = $"{DateTimeOffset.Now:O} [{level}] {message}{Environment.NewLine}";
            lock (SyncRoot)
            {
                File.AppendAllText(LogPath, line);
            }
        }
        catch
        {
            // Diagnostics must never prevent the overlay from starting.
        }
    }
}
