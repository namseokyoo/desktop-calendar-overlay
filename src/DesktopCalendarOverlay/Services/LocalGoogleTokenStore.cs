using System.IO;
using Google.Apis.Util.Store;

namespace DesktopCalendarOverlay.Services;

public sealed class LocalGoogleTokenStore : ITokenStore
{
    public LocalGoogleTokenStore(string? tokenDirectory = null)
    {
        TokenDirectory = tokenDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopCalendarOverlay",
            "google-token-store");
    }

    public string TokenDirectory { get; }

    public bool HasStoredToken
    {
        get
        {
            try
            {
                return Directory.Exists(TokenDirectory) &&
                    Directory.EnumerateFiles(TokenDirectory, "*", SearchOption.AllDirectories).Any();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                AppDiagnostics.Error("Unable to inspect Google Calendar token store.", ex);
                return false;
            }
        }
    }

    public IDataStore CreateDataStore() => new FileDataStore(TokenDirectory, fullPath: true);

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Directory.Exists(TokenDirectory))
        {
            ClearReadOnlyAttributes(TokenDirectory);
            Directory.Delete(TokenDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }

    private static void ClearReadOnlyAttributes(string directory)
    {
        foreach (var path in Directory.EnumerateFileSystemEntries(directory, "*", SearchOption.AllDirectories))
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            }
        }
    }
}
