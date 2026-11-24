using LiquidPing.Core.Models;

namespace LiquidPing.Core.Probes;

public interface IProbe
{
    ProbeKind Kind { get; }

    Task<ProbeResult> ProbeAsync(Host host, TimeSpan timeout, CancellationToken ct);
}
