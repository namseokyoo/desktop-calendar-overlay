using System.IO;
using System.Text.RegularExpressions;

namespace DesktopCalendarOverlay.Services;

public static class AppDiagnostics
{
    private static readonly object SyncRoot = new();
    private static readonly Regex QuotedSensitiveValuePattern = new(
        "(?i)((?:\"|')?(?:access_token|refresh_token|id_token|client_secret|authorization_code|code)(?:\"|')?\\s*[:=]\\s*(?:\"|'))[^\"'\\r\\n]+((?:\"|'))",
        RegexOptions.Compiled);
    private static readonly Regex UnquotedSensitiveValuePattern = new(
        "(?i)((?:\"|')?(?:access_token|refresh_token|id_token|client_secret|authorization_code|code)(?:\"|')?\\s*[:=]\\s*)[^\\s,&\"']+",
        RegexOptions.Compiled);
    private static readonly Regex AuthorizationHeaderPattern = new(
        "(?i)((?:\"|')?authorization(?:\"|')?\\s*[:=]\\s*(?:\"|')?)(bearer|basic)\\s+[^\\s,&\"']+",
        RegexOptions.Compiled);

    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DesktopCalendarOverlay",
        "logs");

    public static string LogPath { get; } = Path.Combine(LogDirectory, "startup.log");

    public static void Info(string message) => Write("INFO", Sanitize(message));

    public static void Error(string message, Exception exception) =>
        Write("ERROR", $"{Sanitize(message)}{Environment.NewLine}{FormatException(exception)}");

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

    private static string FormatException(Exception exception)
    {
        var lines = new List<string>();
        var current = exception;
        var depth = 0;
        while (current is not null && depth < 4)
        {
            lines.Add($"{current.GetType().FullName}: {Sanitize(current.Message)}");
            current = current.InnerException;
            depth++;
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Sanitize(string value)
    {
        var sanitized = QuotedSensitiveValuePattern.Replace(value, "$1[redacted]$2");
        sanitized = UnquotedSensitiveValuePattern.Replace(sanitized, "$1[redacted]");
        sanitized = AuthorizationHeaderPattern.Replace(sanitized, "$1$2 [redacted]");
        return sanitized;
    }
}
