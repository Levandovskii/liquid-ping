namespace LiquidPing.Core.Models;

public sealed class ProbeResult
{
    public ProbeKind Kind { get; init; }
    public bool Success { get; init; }
    public TimeSpan Latency { get; init; }
    public ProbeError Error { get; init; }
    public long BytesSent { get; init; }
    public long BytesReceived { get; init; }
    public string? Detail { get; init; }

    public static ProbeResult Ok(
        ProbeKind kind,
        TimeSpan latency,
        long bytesSent,
        long bytesReceived,
        string? detail = null)
    {
        return new ProbeResult
        {
            Kind = kind,
            Success = true,
            Latency = latency,
            Error = ProbeError.None,
            BytesSent = bytesSent,
            BytesReceived = bytesReceived,
            Detail = detail
        };
    }

    public static ProbeResult Fail(
        ProbeKind kind,
        TimeSpan latency,
        ProbeError error,
        long bytesSent = 0,
        long bytesReceived = 0,
        string? detail = null)
    {
        return new ProbeResult
        {
            Kind = kind,
            Success = false,
            Latency = latency,
            Error = error,
            BytesSent = bytesSent,
            BytesReceived = bytesReceived,
            Detail = detail
        };
    }
}
