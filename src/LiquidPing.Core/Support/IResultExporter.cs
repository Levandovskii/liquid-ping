using LiquidPing.Core.Models;

namespace LiquidPing.Core.Support;

public interface IResultExporter
{
    Task ExportAsync(
        string path,
        IReadOnlyList<HostSnapshot> snapshots,
        CancellationToken ct);
}
