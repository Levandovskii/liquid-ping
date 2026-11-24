using LiquidPing.Core.Models;

namespace LiquidPing.Core.Storage;

public sealed class JsonHostRepository : IHostRepository
{
    private readonly JsonFileStore m_store;
    private readonly string m_path;

    public JsonHostRepository(JsonFileStore store)
        : this(store, AppPaths.HostsFile)
    {
    }

    public JsonHostRepository(JsonFileStore store, string path)
    {
        m_store = store ?? throw new ArgumentNullException(nameof(store));
        m_path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public async Task<IReadOnlyList<Host>> LoadAsync(CancellationToken ct)
    {
        var document = await m_store.ReadAsync<HostsDocument>(m_path, ct).ConfigureAwait(false);

        if (document?.Hosts is null)
        {
            return DefaultHosts.Create();
        }

        return document.Hosts;
    }

    public Task SaveAsync(IReadOnlyList<Host> hosts, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(hosts);
        var document = new HostsDocument(1, hosts.ToList());
        return m_store.WriteAsync(m_path, document, ct);
    }

    private sealed record HostsDocument(int Version, List<Host> Hosts);
}
