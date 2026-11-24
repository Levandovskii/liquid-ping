using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using LiquidPing.Core.Models;

namespace LiquidPing.Core.Scanning;

public sealed class PortScanner : IPortScanner
{
    private const int MAX_PARALLEL_CONNECTIONS = 100;
    private static readonly TimeSpan PORT_TIMEOUT = TimeSpan.FromSeconds(1);

    private static readonly IReadOnlyDictionary<int, string> SERVICES = new Dictionary<int, string>
    {
        [21] = "ftp",
        [22] = "ssh",
        [23] = "telnet",
        [25] = "smtp",
        [53] = "dns",
        [80] = "http",
        [110] = "pop3",
        [143] = "imap",
        [443] = "https",
        [445] = "microsoft-ds",
        [3306] = "mysql",
        [3389] = "rdp",
        [5432] = "postgresql",
        [6379] = "redis",
        [8080] = "http-alt",
        [8443] = "https-alt"
    };

    public async Task<PortScanResult> ScanAsync(
        Host host,
        PortSet ports,
        IProgress<PortScanProgress>? progress,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(ports);

        var stopwatch = Stopwatch.StartNew();
        var total = ports.Ports.Count;
        var completed = 0;
        var cancelled = false;
        var results = new ConcurrentBag<PortResult>();
        using var semaphore = new SemaphoreSlim(MAX_PARALLEL_CONNECTIONS);

        var tasks = ports.Ports.Select(async port =>
        {
            await semaphore.WaitAsync(ct).ConfigureAwait(false);

            try
            {
                var result = await CheckPortAsync(host.Address, port, ct).ConfigureAwait(false);
                results.Add(result);
            }
            finally
            {
                semaphore.Release();
                var current = Interlocked.Increment(ref completed);
                progress?.Report(new PortScanProgress(current, total));
            }
        }).ToList();

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            cancelled = true;
        }

        stopwatch.Stop();
        var ordered = results.OrderBy(result => result.Port).ToArray();
        return new PortScanResult(host, ordered, cancelled, stopwatch.Elapsed);
    }

    private static async Task<PortResult> CheckPortAsync(string address, int port, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var client = new TcpClient();
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutSource.CancelAfter(PORT_TIMEOUT);

            await client.ConnectAsync(address, port, timeoutSource.Token).ConfigureAwait(false);
            stopwatch.Stop();
            return new PortResult(port, PortState.Open, stopwatch.Elapsed, FindService(port));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return new PortResult(port, PortState.Filtered, stopwatch.Elapsed, FindService(port));
        }
        catch (SocketException exception)
        {
            stopwatch.Stop();
            var state = exception.SocketErrorCode == SocketError.ConnectionRefused
                ? PortState.Closed
                : PortState.Filtered;

            return new PortResult(port, state, stopwatch.Elapsed, FindService(port));
        }
    }

    private static string? FindService(int port)
    {
        return SERVICES.TryGetValue(port, out var name) ? name : null;
    }
}
