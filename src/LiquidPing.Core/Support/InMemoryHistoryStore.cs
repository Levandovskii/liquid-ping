using System.Collections.Concurrent;

namespace LiquidPing.Core.Support;

public sealed class InMemoryHistoryStore : IHistoryStore
{
    private const int MAX_POINTS = 1000;

    private readonly ConcurrentDictionary<Guid, Queue<HistoryPoint>> m_buffers = new();
    private readonly object m_gate = new();

    public void Append(Guid hostId, HistoryPoint point)
    {
        var buffer = m_buffers.GetOrAdd(hostId, _ => new Queue<HistoryPoint>());

        lock (m_gate)
        {
            buffer.Enqueue(point);

            while (buffer.Count > MAX_POINTS)
            {
                buffer.Dequeue();
            }
        }
    }

    public IReadOnlyList<HistoryPoint> Get(Guid hostId, TimeSpan window)
    {
        if (!m_buffers.TryGetValue(hostId, out var buffer))
        {
            return Array.Empty<HistoryPoint>();
        }

        var threshold = DateTimeOffset.Now - window;

        lock (m_gate)
        {
            return buffer.Where(point => point.Timestamp >= threshold).ToArray();
        }
    }
}
