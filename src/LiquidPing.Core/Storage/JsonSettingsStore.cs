using LiquidPing.Core.Models;

namespace LiquidPing.Core.Storage;

public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly JsonFileStore m_store;
    private readonly string m_path;

    public JsonSettingsStore(JsonFileStore store)
        : this(store, AppPaths.SettingsFile)
    {
    }

    public JsonSettingsStore(JsonFileStore store, string path)
    {
        m_store = store ?? throw new ArgumentNullException(nameof(store));
        m_path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public async Task<AppSettings> LoadAsync(CancellationToken ct)
    {
        var settings = await m_store.ReadAsync<AppSettings>(m_path, ct).ConfigureAwait(false);
        return settings ?? new AppSettings();
    }

    public Task SaveAsync(AppSettings settings, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return m_store.WriteAsync(m_path, settings, ct);
    }
}
