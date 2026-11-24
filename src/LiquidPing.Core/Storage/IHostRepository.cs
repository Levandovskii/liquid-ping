using LiquidPing.Core.Models;

namespace LiquidPing.Core.Storage;

public interface IHostRepository
{
    Task<IReadOnlyList<Host>> LoadAsync(CancellationToken ct);

    Task SaveAsync(IReadOnlyList<Host> hosts, CancellationToken ct);
}
