using System.IO;
using Google.Apis.Auth.OAuth2;

namespace DesktopCalendarOverlay.Services;

public sealed class CompositeOAuthClientProvider : IOAuthClientProvider
{
    private readonly IReadOnlyList<IOAuthClientProvider> _providers;

    public CompositeOAuthClientProvider(params IOAuthClientProvider[] providers)
    {
        _providers = providers.Length > 0
            ? providers
            : [new OfficialGoogleOAuthClientProvider(), new LocalJsonOAuthClientProvider()];
    }

    public OAuthClientAvailability Availability => SelectedProvider?.Availability ?? OAuthClientAvailability.Missing;

    public string ClientSecretPath => SelectedProvider?.ClientSecretPath
        ?? _providers.FirstOrDefault()?.ClientSecretPath
        ?? OfficialGoogleOAuthClientProvider.OfficialClientFileName;

    public bool IsClientSecretAvailable => SelectedProvider is not null;

    public Task<ClientSecrets> LoadClientSecretsAsync(CancellationToken cancellationToken = default)
    {
        var selected = SelectedProvider;
        if (selected is null)
        {
            throw new FileNotFoundException("No Google OAuth desktop client JSON was found.", ClientSecretPath);
        }

        return selected.LoadClientSecretsAsync(cancellationToken);
    }

    private IOAuthClientProvider? SelectedProvider => _providers.FirstOrDefault(provider => provider.IsClientSecretAvailable);
}
