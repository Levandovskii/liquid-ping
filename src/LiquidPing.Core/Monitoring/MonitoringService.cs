using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using LiquidPing.Core.Models;
using LiquidPing.Core.Probes;

namespace LiquidPing.Core.Monitoring;

public sealed class MonitoringService : IMonitoringService, IDisposable
{
    private const int MAX_PARALLEL_HOSTS = 64;
    private static readonly TimeSpan ICMP_TIMEOUT = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan TCP_TIMEOUT = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan HTTP_TIMEOUT = TimeSpan.FromSeconds(5);

    private readonly object m_gate = new();
    private readonly IReadOnlyList<IProbe> m_probes;
    private readonly StatusEvaluator m_evaluator;
    private readonly ConcurrentDictionary<Guid, HostResultWindow> m_windows = new();
    private Host[] m_hosts = Array.Empty<Host>();
    private CancellationTokenSource? m_loopSource;
    private Task? m_loop;

    public MonitoringService(IEnumerable<IProbe> probes, StatusEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(probes);
        m_probes = probes.ToList();
        m_evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public event EventHandler<HostUpdatedEventArgs>? HostUpdated;

    public void UpdateHosts(IReadOnlyList<Host> hosts)
    {
        ArgumentNullException.ThrowIfNull(hosts);
        Volatile.Write(ref m_hosts, hosts.ToArray());
    }

    public void Start(TimeSpan interval)
    {
        if (interval < TimeSpan.FromSeconds(1))
        {
            throw new ArgumentOutOfRangeException(nameof(interval));
        }

        lock (m_gate)
        {
            if (m_loop is not null && !m_loop.IsCompleted)
            {
                return;
            }

            m_loopSource = new CancellationTokenSource();
            m_loop = RunAsync(interval, m_loopSource.Token);
        }
    }

    public void Stop()
    {
        lock (m_gate)
        {
            m_loopSource?.Cancel();
            m_loopSource?.Dispose();
            m_loopSource = null;
            m_loop = null;
        }
    }

    public async Task CheckOnceAsync(CancellationToken ct)
    {
        var hosts = Volatile.Read(ref m_hosts);

        if (hosts.Length == 0)
        {
            return;
        }

        using var semaphore = new SemaphoreSlim(MAX_PARALLEL_HOSTS);
        var tasks = hosts.Select(host => CheckHostAsync(host, semaphore, ct));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task RunAsync(TimeSpan interval, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                await CheckOnceAsync(ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task CheckHostAsync(Host host, SemaphoreSlim semaphore, CancellationToken ct)
    {
        await semaphore.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            var results = await RunProbesAsync(host, ct).ConfigureAwait(false);
            var resolved = await ResolveAsync(host.Address, ct).ConfigureAwait(false);
            var success = results.Any(result => result.Success);
            var latency = results
                .Where(result => result.Success)
                .Select(result => (TimeSpan?)result.Latency)
                .DefaultIfEmpty(null)
                .Min();

            var window = m_windows.GetOrAdd(host.Id, _ => new HostResultWindow(m_evaluator.WindowSize));
            window.AddCycle(success);
            var status = m_evaluator.Evaluate(window);

            var snapshot = new HostSnapshot
            {
                Host = host,
                Status = status,
                Latency = latency,
                PacketLoss = window.PacketLossPercent(),
                ResolvedAddress = resolved,
                Timestamp = DateTimeOffset.Now
            };

            HostUpdated?.Invoke(this, new HostUpdatedEventArgs(snapshot, results));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<IReadOnlyList<ProbeResult>> RunProbesAsync(Host host, CancellationToken ct)
    {
        var results = new List<ProbeResult>();

        foreach (var probe in SelectProbes(host))
        {
            var timeout = probe.Kind switch
            {
                ProbeKind.Icmp => ICMP_TIMEOUT,
                ProbeKind.Tcp => TCP_TIMEOUT,
                ProbeKind.Http => HTTP_TIMEOUT,
                _ => TCP_TIMEOUT
            };

            try
            {
                results.Add(await probe.ProbeAsync(host, timeout, ct).ConfigureAwait(false));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                results.Add(ProbeResult.Fail(probe.Kind, TimeSpan.Zero, ProbeError.Unreachable));
            }
        }

        return results;
    }

    private IEnumerable<IProbe> SelectProbes(Host host)
    {
        foreach (var probe in m_probes)
        {
            var enabled = probe.Kind switch
            {
                ProbeKind.Icmp => host.UseIcmp,
                ProbeKind.Tcp => host.UseTcp,
                ProbeKind.Http => host.UseHttp,
                _ => false
            };

            if (enabled)
            {
                yield return probe;
            }
        }
    }

    private static async Task<IPAddress?> ResolveAsync(string address, CancellationToken ct)
    {
        if (IPAddress.TryParse(address, out var parsed))
        {
            return parsed;
        }

        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(2));
            var addresses = await Dns
                .GetHostAddressesAsync(address, timeoutSource.Token)
                .ConfigureAwait(false);

            return addresses.FirstOrDefault(item => item.AddressFamily == AddressFamily.InterNetwork)
                ?? addresses.FirstOrDefault();
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            return null;
        }
    }
}
