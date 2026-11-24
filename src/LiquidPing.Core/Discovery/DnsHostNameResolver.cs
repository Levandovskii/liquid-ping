using System.Net;

namespace LiquidPing.Core.Discovery;

public sealed class DnsHostNameResolver : IHostNameResolver
{
    private static readonly TimeSpan RESOLVE_TIMEOUT = TimeSpan.FromSeconds(1);

    public async Task<string?> ResolveAsync(IPAddress address, CancellationToken ct)
    {
        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutSource.CancelAfter(RESOLVE_TIMEOUT);
            var entry = await Dns.GetHostEntryAsync(address).WaitAsync(timeoutSource.Token).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(entry.HostName) ? null : entry.HostName;
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            return null;
        }
    }
}
