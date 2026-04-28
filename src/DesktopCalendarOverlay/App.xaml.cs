using System.Windows;
using DesktopCalendarOverlay.Services;

namespace DesktopCalendarOverlay;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDiagnostics.Info("Application startup requested.");

        DispatcherUnhandledException += (_, args) =>
        {
            AppDiagnostics.Error("Unhandled dispatcher exception.", args.Exception);
            MessageBox.Show(
                $"Desktop Calendar Overlay failed to start or render.\n\n{args.Exception.Message}\n\nA diagnostic log was written to:\n{AppDiagnostics.LogPath}",
                "Desktop Calendar Overlay",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
            Shutdown(-1);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                AppDiagnostics.Error("Unhandled app-domain exception.", exception);
            }
            else
            {
                AppDiagnostics.Info($"Unhandled app-domain exception object: {args.ExceptionObject}");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            AppDiagnostics.Error("Unobserved task exception.", args.Exception);
            args.SetObserved();
        };

        base.OnStartup(e);
    }
}
