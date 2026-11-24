using LiquidPing.Core.Models;

namespace LiquidPing.Core.Storage;

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken ct);

    Task SaveAsync(AppSettings settings, CancellationToken ct);
}
