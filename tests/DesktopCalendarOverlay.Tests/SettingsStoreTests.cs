using DesktopCalendarOverlay.Models;
using DesktopCalendarOverlay.Services;
using Xunit;

namespace DesktopCalendarOverlay.Tests;

public sealed class SettingsStoreTests
{
    [Fact]
    public void RoundTripsSettings()
    {
        using var directory = TemporaryDirectory.Create();
        var store = new JsonSettingsStore(directory.Path);
        var settings = new CalendarOverlaySettings(
            EventDisplayMode: CalendarEventDisplayModes.EventFirst,
            OverlayOpacity: 0.72,
            ThemeName: CalendarThemeNames.IvoryEditorial,
            EventListFontSize: 12,
            IsPositionLocked: true,
            StartWithWindows: true);

        store.Write("overlay-ui-settings", settings);
        var actual = store.Read<CalendarOverlaySettings>("overlay-ui-settings");

        Assert.NotNull(actual);
        Assert.Equal(settings, actual);
    }

    [Fact]
    public void DeleteRemovesSettingsFile()
    {
        using var directory = TemporaryDirectory.Create();
        var store = new JsonSettingsStore(directory.Path);

        store.Write("overlay-ui-settings", new CalendarOverlaySettings(IsPositionLocked: true));
        store.Delete("overlay-ui-settings");

        Assert.Null(store.Read<CalendarOverlaySettings>("overlay-ui-settings"));
    }

    [Fact]
    public void InvalidJsonFallsBackToDefault()
    {
        using var directory = TemporaryDirectory.Create();
        var store = new JsonSettingsStore(directory.Path);
        File.WriteAllText(Path.Combine(directory.Path, "overlay-ui-settings.json"), "{ invalid json");

        var actual = store.Read<CalendarOverlaySettings>("overlay-ui-settings");

        Assert.Null(actual);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDirectory Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dco-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            return new TemporaryDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
