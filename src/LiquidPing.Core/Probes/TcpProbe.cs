using System.Diagnostics;
using System.Net.Sockets;
using LiquidPing.Core.Models;

namespace LiquidPing.Core.Probes;

public sealed class TcpProbe : IProbe
{
    private const long SYN_BYTES = 74;
    private const long SYN_ACK_BYTES = 74;

    public ProbeKind Kind => ProbeKind.Tcp;

    public async Task<ProbeResult> ProbeAsync(Host host, TimeSpan timeout, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var client = new TcpClient();
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutSource.CancelAfter(timeout);

            await client
                .ConnectAsync(host.Address, host.TcpPort, timeoutSource.Token)
                .ConfigureAwait(false);

            stopwatch.Stop();
            return ProbeResult.Ok(Kind, stopwatch.Elapsed, SYN_BYTES, SYN_ACK_BYTES);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return ProbeResult.Fail(Kind, stopwatch.Elapsed, ProbeError.Timeout, SYN_BYTES);
        }
        catch (SocketException exception)
        {
            stopwatch.Stop();
            return ProbeResult.Fail(Kind, stopwatch.Elapsed, MapSocketError(exception.SocketErrorCode), SYN_BYTES);
        }
    }

    private static ProbeError MapSocketError(SocketError error)
    {
        return error switch
        {
            SocketError.ConnectionRefused => ProbeError.Refused,
            SocketError.HostNotFound => ProbeError.DnsFailure,
            SocketError.NoData => ProbeError.DnsFailure,
            SocketError.TryAgain => ProbeError.DnsFailure,
            SocketError.TimedOut => ProbeError.Timeout,
            SocketError.NetworkUnreachable => ProbeError.Unreachable,
            SocketError.HostUnreachable => ProbeError.Unreachable,
            _ => ProbeError.Unreachable
        };
    }
}
