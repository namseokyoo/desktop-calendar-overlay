namespace DesktopCalendarOverlay.Services;

public interface IGoogleCalendarIntegration
{
    string ClientSecretPath { get; }

    string TokenDirectory { get; }

    bool IsClientSecretAvailable { get; }

    bool HasStoredToken { get; }

    bool IsUsingGoogle { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);

    Task SetLayerVisibilityAsync(string calendarLayerId, bool isVisible, CancellationToken cancellationToken = default);
}
