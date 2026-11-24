namespace LiquidPing.Core.Monitoring;

public sealed class HostResultWindow
{
    private readonly object m_gate = new();
    private readonly Queue<bool> m_cycles = new();
    private readonly int m_capacity;

    public HostResultWindow(int capacity = 20)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        m_capacity = capacity;
    }

    public int Count
    {
        get
        {
            lock (m_gate)
            {
                return m_cycles.Count;
            }
        }
    }

    public int ConsecutiveFailures
    {
        get
        {
            lock (m_gate)
            {
                var failures = 0;

                foreach (var success in m_cycles.Reverse())
                {
                    if (success)
                    {
                        break;
                    }

                    failures++;
                }

                return failures;
            }
        }
    }

    public void AddCycle(bool success)
    {
        lock (m_gate)
        {
            m_cycles.Enqueue(success);

            while (m_cycles.Count > m_capacity)
            {
                m_cycles.Dequeue();
            }
        }
    }

    public double PacketLossPercent()
    {
        lock (m_gate)
        {
            if (m_cycles.Count == 0)
            {
                return 0;
            }

            var failed = m_cycles.Count(value => !value);
            return failed * 100.0 / m_cycles.Count;
        }
    }
}
