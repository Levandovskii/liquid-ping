using System.Diagnostics;
using System.Net.NetworkInformation;
using LiquidPing.Core.Models;

namespace LiquidPing.Core.Probes;

public sealed class IcmpProbe : IProbe
{
    private const int PAYLOAD_BYTES = 32;
    private const int ICMP_HEADER_BYTES = 8;

    public ProbeKind Kind => ProbeKind.Icmp;

    public async Task<ProbeResult> ProbeAsync(Host host, TimeSpan timeout, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var bytesSent = PAYLOAD_BYTES + ICMP_HEADER_BYTES;

        try
        {
            using var ping = new Ping();
            var reply = await ping
                .SendPingAsync(host.Address, ToTimeout(timeout))
                .WaitAsync(ct)
                .ConfigureAwait(false);

            stopwatch.Stop();

            if (reply.Status == IPStatus.Success)
            {
                return ProbeResult.Ok(
                    Kind,
                    stopwatch.Elapsed,
                    bytesSent,
                    bytesSent + reply.Buffer.Length,
                    reply.Address?.ToString());
            }

            return ProbeResult.Fail(Kind, stopwatch.Elapsed, MapStatus(reply.Status), bytesSent);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return ProbeResult.Fail(Kind, stopwatch.Elapsed, ProbeError.Timeout, bytesSent);
        }
        catch (PingException)
        {
            stopwatch.Stop();
            return ProbeResult.Fail(Kind, stopwatch.Elapsed, ProbeError.Unreachable, bytesSent);
        }
    }

    private static int ToTimeout(TimeSpan timeout)
    {
        return (int)Math.Clamp(timeout.TotalMilliseconds, 1, int.MaxValue);
    }

    private static ProbeError MapStatus(IPStatus status)
    {
        return status switch
        {
            IPStatus.TimedOut => ProbeError.Timeout,
            IPStatus.DestinationNetworkUnreachable => ProbeError.Unreachable,
            IPStatus.DestinationHostUnreachable => ProbeError.Unreachable,
            IPStatus.DestinationPortUnreachable => ProbeError.Unreachable,
            IPStatus.DestinationProhibited => ProbeError.Unreachable,
            _ => ProbeError.Unreachable
        };
    }
}
