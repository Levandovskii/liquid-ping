namespace LiquidPing.Core.Discovery;

public interface ILanDiscovery
{
    Task<IReadOnlyList<LanHostInfo>> DiscoverAsync(
        IProgress<LanScanProgress>? progress,
        CancellationToken ct);
}
