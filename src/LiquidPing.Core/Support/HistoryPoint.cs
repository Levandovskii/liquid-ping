namespace LiquidPing.Core.Support;

public sealed record HistoryPoint(DateTimeOffset Timestamp, TimeSpan? Latency);
