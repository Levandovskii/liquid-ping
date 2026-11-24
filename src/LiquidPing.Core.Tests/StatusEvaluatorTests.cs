using LiquidPing.Core.Models;
using LiquidPing.Core.Monitoring;

namespace LiquidPing.Core.Tests;

public sealed class StatusEvaluatorTests
{
    private readonly StatusEvaluator m_evaluator = new();

    [Fact]
    public void Evaluate_EmptyWindow_ReturnsPending()
    {
        var window = new HostResultWindow();

        Assert.Equal(HostStatus.Pending, m_evaluator.Evaluate(window));
    }

    [Fact]
    public void Evaluate_StableReplies_ReturnsAvailable()
    {
        var window = new HostResultWindow();

        for (var index = 0; index < 10; index++)
        {
            window.AddCycle(index != 5);
        }

        Assert.Equal(HostStatus.Available, m_evaluator.Evaluate(window));
    }

    [Fact]
    public void Evaluate_LossAboveTenPercent_ReturnsUnstable()
    {
        var window = new HostResultWindow();

        for (var index = 0; index < 10; index++)
        {
            window.AddCycle(index >= 3);
        }

        Assert.Equal(HostStatus.Unstable, m_evaluator.Evaluate(window));
    }

    [Fact]
    public void Evaluate_ThreeFailuresInRow_ReturnsUnavailable()
    {
        var window = new HostResultWindow();
        window.AddCycle(true);
        window.AddCycle(false);
        window.AddCycle(false);
        window.AddCycle(false);

        Assert.Equal(HostStatus.Unavailable, m_evaluator.Evaluate(window));
    }
}
