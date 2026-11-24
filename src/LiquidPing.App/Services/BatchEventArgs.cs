using LiquidPing.Core.Models;

namespace LiquidPing.App.Services;

public sealed class BatchEventArgs : EventArgs
{
    public BatchEventArgs(IReadOnlyList<HostSnapshot> snapshots)
    {
        Snapshots = snapshots;
    }

    public IReadOnlyList<HostSnapshot> Snapshots { get; }
}
