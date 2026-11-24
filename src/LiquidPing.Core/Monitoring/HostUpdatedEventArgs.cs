using LiquidPing.Core.Models;

namespace LiquidPing.Core.Monitoring;

public sealed class HostUpdatedEventArgs : EventArgs
{
    public HostUpdatedEventArgs(HostSnapshot snapshot, IReadOnlyList<ProbeResult> results)
    {
        Snapshot = snapshot;
        Results = results;
    }

    public HostSnapshot Snapshot { get; }

    public IReadOnlyList<ProbeResult> Results { get; }
}
