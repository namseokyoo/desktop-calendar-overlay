using Google.Apis.Util.Store;

namespace DesktopCalendarOverlay.Services;

public interface ITokenStore
{
    string TokenDirectory { get; }

    bool HasStoredToken { get; }

    IDataStore CreateDataStore();

    Task ClearAsync(CancellationToken cancellationToken = default);
}
