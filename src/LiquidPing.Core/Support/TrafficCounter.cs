using LiquidPing.Core.Models;

namespace LiquidPing.Core.Support;

public sealed class TrafficCounter
{
    private long m_sent;
    private long m_received;

    public long Sent => Interlocked.Read(ref m_sent);

    public long Received => Interlocked.Read(ref m_received);

    public void Add(ProbeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Interlocked.Add(ref m_sent, result.BytesSent);
        Interlocked.Add(ref m_received, result.BytesReceived);
    }

    public void Reset()
    {
        Interlocked.Exchange(ref m_sent, 0);
        Interlocked.Exchange(ref m_received, 0);
    }
}
