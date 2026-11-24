using System.Net;

namespace LiquidPing.Core.Discovery;

public interface IHostNameResolver
{
    Task<string?> ResolveAsync(IPAddress address, CancellationToken ct);
}
