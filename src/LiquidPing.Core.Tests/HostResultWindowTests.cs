using LiquidPing.Core.Monitoring;

namespace LiquidPing.Core.Tests;

public sealed class HostResultWindowTests
{
    [Fact]
    public void PacketLossPercent_CountsFailedCycles()
    {
        var window = new HostResultWindow(20);
        window.AddCycle(true);
        window.AddCycle(true);
        window.AddCycle(false);
        window.AddCycle(false);

        Assert.Equal(50, window.PacketLossPercent(), 3);
        Assert.Equal(2, window.ConsecutiveFailures);
        Assert.Equal(4, window.Count);
    }

    [Fact]
    public void AddCycle_DropsOldestValuesAboveCapacity()
    {
        var window = new HostResultWindow(3);
        window.AddCycle(false);
        window.AddCycle(false);
        window.AddCycle(true);
        window.AddCycle(true);

        Assert.Equal(3, window.Count);
        Assert.Equal(0, window.ConsecutiveFailures);
        Assert.Equal(100.0 / 3, window.PacketLossPercent(), 3);
    }

    [Fact]
    public void ConsecutiveFailures_ResetsAfterSuccess()
    {
        var window = new HostResultWindow();
        window.AddCycle(false);
        window.AddCycle(false);
        window.AddCycle(true);
        window.AddCycle(false);

        Assert.Equal(1, window.ConsecutiveFailures);
    }
}
