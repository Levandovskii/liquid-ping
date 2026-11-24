using LiquidPing.Core.Models;

namespace LiquidPing.Core.Scanning;

public sealed record PortScanResult(
    Host Host,
    IReadOnlyList<PortResult> Results,
    bool Cancelled,
    TimeSpan Duration);
