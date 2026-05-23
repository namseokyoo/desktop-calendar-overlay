using System.IO;
using Google.Apis.Auth.OAuth2;

namespace DesktopCalendarOverlay.Services;

public sealed class LocalJsonOAuthClientProvider : IOAuthClientProvider
{
    public LocalJsonOAuthClientProvider(string? clientSecretPath = null)
    {
        ClientSecretPath = clientSecretPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopCalendarOverlay",
            "google-oauth-client.json");
    }

    public OAuthClientAvailability Availability => IsClientSecretAvailable
        ? OAuthClientAvailability.LocalJson
        : OAuthClientAvailability.Missing;

    public string ClientSecretPath { get; }

    public bool IsClientSecretAvailable => File.Exists(ClientSecretPath);

    public Task<ClientSecrets> LoadClientSecretsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsClientSecretAvailable)
        {
            throw new FileNotFoundException("Google OAuth desktop client JSON was not found.", ClientSecretPath);
        }

        using var stream = File.OpenRead(ClientSecretPath);
        return Task.FromResult(GoogleClientSecrets.FromStream(stream).Secrets);
    }
}
