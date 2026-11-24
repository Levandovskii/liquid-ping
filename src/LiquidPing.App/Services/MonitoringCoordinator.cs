using System.Threading.Channels;
using LiquidPing.Core.Models;
using LiquidPing.Core.Monitoring;

namespace LiquidPing.App.Services;

public sealed class MonitoringCoordinator : IDisposable
{
    private static readonly TimeSpan BATCH_INTERVAL = TimeSpan.FromMilliseconds(150);

    private readonly IMonitoringService m_monitoring;
    private readonly Channel<HostSnapshot> m_channel = Channel.CreateUnbounded<HostSnapshot>();
    private readonly object m_gate = new();
    private CancellationTokenSource? m_source;
    private Task? m_pump;

    public MonitoringCoordinator(IMonitoringService monitoring)
    {
        m_monitoring = monitoring ?? throw new ArgumentNullException(nameof(monitoring));
        m_monitoring.HostUpdated += OnHostUpdated;
    }

    public event EventHandler<BatchEventArgs>? BatchReady;

    public void Start(TimeSpan interval)
    {
        lock (m_gate)
        {
            if (m_pump is not null && !m_pump.IsCompleted)
            {
                return;
            }

            m_source = new CancellationTokenSource();
            m_pump = PumpAsync(m_source.Token);
            m_monitoring.Start(interval);
        }
    }

    public void Stop()
    {
        lock (m_gate)
        {
            m_monitoring.Stop();
            m_source?.Cancel();
            m_source?.Dispose();
            m_source = null;
            m_pump = null;
        }
    }

    public void Dispose()
    {
        Stop();
        m_monitoring.HostUpdated -= OnHostUpdated;
    }

    private void OnHostUpdated(object? sender, HostUpdatedEventArgs args)
    {
        m_channel.Writer.TryWrite(args.Snapshot);
    }

    private async Task PumpAsync(CancellationToken ct)
    {
        var batch = new List<HostSnapshot>();
        using var timer = new PeriodicTimer(BATCH_INTERVAL);

        try
        {
            while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                while (m_channel.Reader.TryRead(out var snapshot))
                {
                    batch.Add(snapshot);
                }

                if (batch.Count == 0)
                {
                    continue;
                }

                BatchReady?.Invoke(this, new BatchEventArgs(batch.ToArray()));
                batch.Clear();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
