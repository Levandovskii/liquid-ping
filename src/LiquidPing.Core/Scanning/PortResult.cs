namespace LiquidPing.Core.Scanning;

public sealed record PortResult(int Port, PortState State, TimeSpan? Latency, string? ServiceName);
