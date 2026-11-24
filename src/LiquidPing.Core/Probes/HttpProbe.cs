using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using LiquidPing.Core.Models;

namespace LiquidPing.Core.Probes;

public sealed class HttpProbe : IProbe
{
    private static readonly HttpClient SHARED_CLIENT = new()
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    private readonly HttpClient m_client;

    public HttpProbe()
        : this(SHARED_CLIENT)
    {
    }

    public HttpProbe(HttpClient client)
    {
        m_client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ProbeKind Kind => ProbeKind.Http;

    public async Task<ProbeResult> ProbeAsync(Host host, TimeSpan timeout, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var uri = BuildUri(host);
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutSource.CancelAfter(timeout);

            using var response = await m_client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutSource.Token)
                .ConfigureAwait(false);

            stopwatch.Stop();
            var received = response.Content.Headers.ContentLength ?? 0;
            var detail = ((int)response.StatusCode).ToString();

            if (!response.IsSuccessStatusCode)
            {
                return ProbeResult.Fail(Kind, stopwatch.Elapsed, ProbeError.HttpError, 0, received, detail);
            }

            return ProbeResult.Ok(Kind, stopwatch.Elapsed, 0, received, detail);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return ProbeResult.Fail(Kind, stopwatch.Elapsed, ProbeError.Timeout);
        }
        catch (HttpRequestException exception)
        {
            stopwatch.Stop();
            return ProbeResult.Fail(Kind, stopwatch.Elapsed, MapHttpError(exception));
        }
        catch (UriFormatException)
        {
            stopwatch.Stop();
            return ProbeResult.Fail(Kind, stopwatch.Elapsed, ProbeError.Unreachable, detail: "invalid uri");
        }
    }

    private static Uri BuildUri(Host host)
    {
        if (!string.IsNullOrWhiteSpace(host.HttpUrl))
        {
            return new Uri(host.HttpUrl);
        }

        return new Uri($"https://{host.Address}/");
    }

    private static ProbeError MapHttpError(HttpRequestException exception)
    {
        if (exception.HttpRequestError == HttpRequestError.NameResolutionError)
        {
            return ProbeError.DnsFailure;
        }

        if (exception.InnerException is SocketException socketException)
        {
            return socketException.SocketErrorCode switch
            {
                SocketError.HostNotFound => ProbeError.DnsFailure,
                SocketError.NoData => ProbeError.DnsFailure,
                SocketError.TimedOut => ProbeError.Timeout,
                _ => ProbeError.Unreachable
            };
        }

        return exception.HttpRequestError switch
        {
            HttpRequestError.ConnectionError => ProbeError.Unreachable,
            HttpRequestError.SecureConnectionError => ProbeError.Unreachable,
            HttpRequestError.Unknown => ProbeError.Unreachable,
            _ => ProbeError.Unreachable
        };
    }
}
