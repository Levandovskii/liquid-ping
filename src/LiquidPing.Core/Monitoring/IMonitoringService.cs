using LiquidPing.Core.Models;

namespace LiquidPing.Core.Monitoring;

public interface IMonitoringService
{
    event EventHandler<HostUpdatedEventArgs>? HostUpdated;

    void UpdateHosts(IReadOnlyList<Host> hosts);

    void Start(TimeSpan interval);

    void Stop();

    Task CheckOnceAsync(CancellationToken ct);
}
