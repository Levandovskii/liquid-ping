using System.Net;

namespace LiquidPing.Core.Models;

public sealed class HostSnapshot
{
    public required Host Host { get; init; }
    public HostStatus Status { get; init; }
    public TimeSpan? Latency { get; init; }
    public double PacketLoss { get; init; }
    public IPAddress? ResolvedAddress { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}
