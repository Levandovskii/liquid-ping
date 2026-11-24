using LiquidPing.Core.Monitoring;

namespace LiquidPing.App.Services;

public sealed class NotificationService
{
    public bool IsEnabled { get; set; } = true;

    public string? LastMessage { get; private set; }

    public void Notify(HostUpdatedEventArgs args)
    {
        if (!IsEnabled || args is null)
        {
            return;
        }

        LastMessage = $"{args.Snapshot.Host.Name}: {args.Snapshot.Status}";
    }
}
