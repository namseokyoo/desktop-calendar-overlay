using DesktopCalendarOverlay.Models;
using DesktopCalendarOverlay.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Xunit;

namespace DesktopCalendarOverlay.Tests;

public sealed class CalendarServiceRouterTests
{
    [Fact]
    public async Task UsesMockCalendarWhenOAuthClientIsMissing()
    {
        var router = CreateRouter(hasStoredToken: false, OAuthClientAvailability.Missing);
        var layers = await router.GetLayersAsync();
        var events = await router.GetEventsAsync(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 8));

        Assert.False(router.IsUsingGoogle);
        Assert.Contains(layers, layer => layer.Id == "primary");
        Assert.NotEmpty(events);
    }

    [Fact]
    public async Task RoutesCrudToMockCalendarUntilGoogleIsConnected()
    {
        var router = CreateRouter(hasStoredToken: false, OAuthClientAvailability.LocalJson);
        var start = new DateTimeOffset(2026, 5, 23, 10, 0, 0, TimeSpan.Zero);
        var calendarEvent = new CalendarEvent("", "primary", "Draft event", start, start.AddMinutes(30));

        var created = await router.CreateEventAsync(calendarEvent);
        var updated = await router.UpdateEventAsync(created with { Title = "Updated event" });
        await router.DeleteEventAsync(updated);

        Assert.False(router.IsUsingGoogle);
        Assert.StartsWith("mock-created-", created.Id, StringComparison.Ordinal);
        Assert.Equal("Updated event", updated.Title);
    }

    [Fact]
    public void ExposesOAuthClientAvailability()
    {
        var router = CreateRouter(hasStoredToken: true, OAuthClientAvailability.LocalJson);

        Assert.True(router.IsUsingGoogle);
        Assert.Equal(OAuthClientAvailability.LocalJson, router.OAuthClientAvailability);
        Assert.Equal("test-client.json", Path.GetFileName(router.ClientSecretPath));
        Assert.Equal("test-token-store", Path.GetFileName(router.TokenDirectory));
    }

    [Fact]
    public void CompositeOAuthClientProviderPrefersOfficialClient()
    {
        var official = new FakeOAuthClientProvider(OAuthClientAvailability.Official, "official-client.json");
        var localJson = new FakeOAuthClientProvider(OAuthClientAvailability.LocalJson, "local-client.json");
        var provider = new CompositeOAuthClientProvider(official, localJson);

        Assert.True(provider.IsClientSecretAvailable);
        Assert.Equal(OAuthClientAvailability.Official, provider.Availability);
        Assert.Equal("official-client.json", Path.GetFileName(provider.ClientSecretPath));
    }

    [Fact]
    public void CompositeOAuthClientProviderFallsBackToLocalJsonForDevelopers()
    {
        var official = new FakeOAuthClientProvider(OAuthClientAvailability.Missing, "official-client.json");
        var localJson = new FakeOAuthClientProvider(OAuthClientAvailability.LocalJson, "local-client.json");
        var provider = new CompositeOAuthClientProvider(official, localJson);

        Assert.True(provider.IsClientSecretAvailable);
        Assert.Equal(OAuthClientAvailability.LocalJson, provider.Availability);
        Assert.Equal("local-client.json", Path.GetFileName(provider.ClientSecretPath));
    }

    private static CalendarServiceRouter CreateRouter(bool hasStoredToken, OAuthClientAvailability availability)
    {
        var settingsStore = new InMemorySettingsStore();
        var tokenStore = new FakeTokenStore(hasStoredToken);
        var oauthProvider = new FakeOAuthClientProvider(availability);
        var googleService = new GoogleCalendarService(settingsStore, tokenStore, oauthProvider);
        return new CalendarServiceRouter(googleService, new MockCalendarService());
    }

    private sealed class FakeTokenStore(bool hasStoredToken) : ITokenStore
    {
        public string TokenDirectory => Path.Combine(Path.GetTempPath(), "test-token-store");

        public bool HasStoredToken => hasStoredToken;

        public IDataStore CreateDataStore() => throw new NotSupportedException("Test token store is not used for live OAuth.");

        public Task ClearAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeOAuthClientProvider(
        OAuthClientAvailability availability,
        string clientSecretFileName = "test-client.json") : IOAuthClientProvider
    {
        public OAuthClientAvailability Availability => availability;

        public string ClientSecretPath => Path.Combine(Path.GetTempPath(), clientSecretFileName);

        public bool IsClientSecretAvailable => availability != OAuthClientAvailability.Missing;

        public Task<ClientSecrets> LoadClientSecretsAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Test OAuth provider is not used for live OAuth.");
    }

    private sealed class InMemorySettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, object> _values = [];

        public T? Read<T>(string key) => _values.TryGetValue(key, out var value) ? (T)value : default;

        public void Write<T>(string key, T value)
        {
            if (value is not null)
            {
                _values[key] = value;
            }
        }

        public void Delete(string key) => _values.Remove(key);
    }
}
