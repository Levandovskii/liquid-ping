using LiquidPing.Core.Models;

namespace LiquidPing.Core.Monitoring;

public sealed class StatusEvaluator
{
    private const double LOSS_THRESHOLD_PERCENT = 10;
    private const int FAILURE_THRESHOLD = 3;

    public StatusEvaluator(int windowSize = 20)
    {
        if (windowSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(windowSize));
        }

        WindowSize = windowSize;
    }

    public int WindowSize { get; }

    public HostStatus Evaluate(HostResultWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (window.Count == 0)
        {
            return HostStatus.Pending;
        }

        if (window.ConsecutiveFailures >= FAILURE_THRESHOLD)
        {
            return HostStatus.Unavailable;
        }

        if (window.PacketLossPercent() > LOSS_THRESHOLD_PERCENT)
        {
            return HostStatus.Unstable;
        }

        return HostStatus.Available;
    }
}
