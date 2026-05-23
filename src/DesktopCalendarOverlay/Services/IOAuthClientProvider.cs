using Google.Apis.Auth.OAuth2;

namespace DesktopCalendarOverlay.Services;

public enum OAuthClientAvailability
{
    Missing,
    LocalJson,
    FutureOfficial
}

public interface IOAuthClientProvider
{
    OAuthClientAvailability Availability { get; }

    string ClientSecretPath { get; }

    bool IsClientSecretAvailable { get; }

    Task<ClientSecrets> LoadClientSecretsAsync(CancellationToken cancellationToken = default);
}
