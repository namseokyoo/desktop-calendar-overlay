using System.IO;
using Microsoft.Win32;

namespace DesktopCalendarOverlay.Services;

public sealed class StartupRegistrationService
{
    private const string StartupKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "DesktopCalendarOverlay";

    public bool IsSupported => OperatingSystem.IsWindows();

    public bool IsEnabled
    {
        get
        {
            if (!IsSupported)
            {
                return false;
            }

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, writable: false);
                return key?.GetValue(ValueName) is string value && !string.IsNullOrWhiteSpace(value);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
            {
                AppDiagnostics.Error("Unable to read Windows startup registration.", ex);
                return false;
            }
        }
        set
        {
            if (!IsSupported)
            {
                return;
            }

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupKeyPath, writable: true)
                    ?? Registry.CurrentUser.CreateSubKey(StartupKeyPath, writable: true);
                if (value)
                {
                    key.SetValue(ValueName, $"\"{Environment.ProcessPath}\"");
                }
                else
                {
                    key.DeleteValue(ValueName, throwOnMissingValue: false);
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
            {
                AppDiagnostics.Error("Unable to update Windows startup registration.", ex);
                throw new InvalidOperationException("Windows startup setting could not be updated. Check Windows account permissions and try again.", ex);
            }
        }
    }
}
