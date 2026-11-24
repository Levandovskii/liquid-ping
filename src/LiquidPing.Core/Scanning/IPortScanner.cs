using LiquidPing.Core.Models;

namespace LiquidPing.Core.Scanning;

public interface IPortScanner
{
    Task<PortScanResult> ScanAsync(
        Host host,
        PortSet ports,
        IProgress<PortScanProgress>? progress,
        CancellationToken ct);
}
