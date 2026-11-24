using LiquidPing.Core.Models;
using LiquidPing.Core.Monitoring;
using LiquidPing.Core.Probes;

namespace LiquidPing.Core.Tests;

public sealed class MonitoringServiceTests
{
    [Fact]
    public async Task CheckOnceAsync_RaisesSnapshotForHost()
    {
        var service = CreateService(new FakeProbe(ProbeKind.Icmp, true));
        var host = CreateHost();
        service.UpdateHosts(new[] { host });
        var received = new List<HostUpdatedEventArgs>();
        service.HostUpdated += (_, args) => received.Add(args);

        await service.CheckOnceAsync(CancellationToken.None);

        var update = Assert.Single(received);
        Assert.Equal(host.Id, update.Snapshot.Host.Id);
        Assert.Equal(HostStatus.Available, update.Snapshot.Status);
        Assert.Equal(10, update.Snapshot.Latency!.Value.TotalMilliseconds, 3);
        Assert.Single(update.Results);
    }

    [Fact]
    public async Task CheckOnceAsync_ThreeFailures_ReportsUnavailable()
    {
        var service = CreateService(new FakeProbe(ProbeKind.Icmp, false));
        service.UpdateHosts(new[] { CreateHost() });
        HostStatus? lastStatus = null;
        service.HostUpdated += (_, args) => lastStatus = args.Snapshot.Status;

        await service.CheckOnceAsync(CancellationToken.None);
        await service.CheckOnceAsync(CancellationToken.None);
        await service.CheckOnceAsync(CancellationToken.None);

        Assert.Equal(HostStatus.Unavailable, lastStatus);
    }

    [Fact]
    public async Task CheckOnceAsync_NoHosts_DoesNotRaiseEvents()
    {
        var service = CreateService(new FakeProbe(ProbeKind.Icmp, true));
        var raised = false;
        service.HostUpdated += (_, _) => raised = true;

        await service.CheckOnceAsync(CancellationToken.None);

        Assert.False(raised);
    }

    [Fact]
    public void Start_InvalidInterval_Throws()
    {
        var service = CreateService(new FakeProbe(ProbeKind.Icmp, true));

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(TimeSpan.Zero));
    }

    [Fact]
    public async Task Start_ChecksHostsPeriodically()
    {
        var service = CreateService(new FakeProbe(ProbeKind.Icmp, true));
        service.UpdateHosts(new[] { CreateHost() });
        var count = 0;
        service.HostUpdated += (_, _) => Interlocked.Increment(ref count);

        service.Start(TimeSpan.FromSeconds(1));
        await Task.Delay(1600);
        service.Stop();

        Assert.True(Volatile.Read(ref count) >= 1);
    }

    private static MonitoringService CreateService(params IProbe[] probes)
    {
        return new MonitoringService(probes, new StatusEvaluator());
    }

    private static Host CreateHost()
    {
        return new Host
        {
            Name = "Local",
            Address = "127.0.0.1",
            UseIcmp = true,
            UseTcp = false,
            UseHttp = false
        };
    }

    private sealed class FakeProbe : IProbe
    {
        private readonly bool m_success;

        public FakeProbe(ProbeKind kind, bool success)
        {
            Kind = kind;
            m_success = success;
        }

        public ProbeKind Kind { get; }

        public Task<ProbeResult> ProbeAsync(Host host, TimeSpan timeout, CancellationToken ct)
        {
            var result = m_success
                ? ProbeResult.Ok(Kind, TimeSpan.FromMilliseconds(10), 40, 40)
                : ProbeResult.Fail(Kind, TimeSpan.Zero, ProbeError.Timeout);

            return Task.FromResult(result);
        }
    }
}
