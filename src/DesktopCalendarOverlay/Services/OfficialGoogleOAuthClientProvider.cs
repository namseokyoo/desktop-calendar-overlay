using System.IO;
using Google.Apis.Auth.OAuth2;

namespace DesktopCalendarOverlay.Services;

public sealed class OfficialGoogleOAuthClientProvider : IOAuthClientProvider
{
    public const string EnvironmentVariableName = "DCO_GOOGLE_OAUTH_CLIENT_JSON";
    public const string OfficialClientFileName = "google-oauth-client.official.json";

    private readonly IReadOnlyList<string> _candidatePaths;

    public OfficialGoogleOAuthClientProvider(IEnumerable<string>? candidatePaths = null)
    {
        _candidatePaths = candidatePaths?.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray()
            ?? BuildDefaultCandidatePaths();
    }

    public OAuthClientAvailability Availability => IsClientSecretAvailable
        ? OAuthClientAvailability.Official
        : OAuthClientAvailability.Missing;

    public string ClientSecretPath => ResolveExistingPath() ?? _candidatePaths.FirstOrDefault() ?? OfficialClientFileName;

    public bool IsClientSecretAvailable => ResolveExistingPath() is not null;

    public Task<ClientSecrets> LoadClientSecretsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolveExistingPath();
        if (path is null)
        {
            throw new FileNotFoundException("Official Google OAuth desktop client JSON was not found.", ClientSecretPath);
        }

        using var stream = File.OpenRead(path);
        return Task.FromResult(GoogleClientSecrets.FromStream(stream).Secrets);
    }

    private string? ResolveExistingPath() => _candidatePaths.FirstOrDefault(File.Exists);

    private static string[] BuildDefaultCandidatePaths()
    {
        var paths = new List<string>();
        var environmentPath = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(environmentPath))
        {
            paths.Add(environmentPath);
        }

        paths.Add(Path.Combine(AppContext.BaseDirectory, OfficialClientFileName));
        paths.Add(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopCalendarOverlay",
            OfficialClientFileName));

        return paths.ToArray();
    }
}
