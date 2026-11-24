using LiquidPing.Core.Models;
using LiquidPing.Core.Support;

namespace LiquidPing.Core.Tests;

public sealed class SupportServicesTests : IDisposable
{
    private readonly string m_directory = Path.Combine(Path.GetTempPath(), "liquidping-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void TrafficCounter_AccumulatesAndResets()
    {
        var counter = new TrafficCounter();
        var result = ProbeResult.Ok(ProbeKind.Icmp, TimeSpan.FromMilliseconds(5), 40, 64);

        counter.Add(result);
        counter.Add(result);

        Assert.Equal(80, counter.Sent);
        Assert.Equal(128, counter.Received);

        counter.Reset();
        Assert.Equal(0, counter.Sent);
        Assert.Equal(0, counter.Received);
    }

    [Fact]
    public void HistoryStore_FiltersByWindow()
    {
        var store = new InMemoryHistoryStore();
        var hostId = Guid.NewGuid();
        store.Append(hostId, new HistoryPoint(DateTimeOffset.Now.AddMinutes(-30), TimeSpan.FromMilliseconds(10)));
        store.Append(hostId, new HistoryPoint(DateTimeOffset.Now, TimeSpan.FromMilliseconds(20)));

        var points = store.Get(hostId, TimeSpan.FromMinutes(10));

        Assert.Single(points);
        Assert.Equal(20, points[0].Latency!.Value.TotalMilliseconds, 3);
    }

    [Fact]
    public async Task CsvExporter_WritesHeaderAndRows()
    {
        var path = Path.Combine(m_directory, "results.csv");
        var exporter = new CsvExporter();

        await exporter.ExportAsync(path, new[] { CreateSnapshot() }, CancellationToken.None);

        var text = await File.ReadAllTextAsync(path);
        Assert.Contains("time,host,status", text);
        Assert.Contains("Google", text);
        Assert.Contains("Available", text);
    }

    [Fact]
    public async Task JsonExporter_WritesValidDocument()
    {
        var path = Path.Combine(m_directory, "results.json");
        var exporter = new JsonExporter();

        await exporter.ExportAsync(path, new[] { CreateSnapshot() }, CancellationToken.None);

        var text = await File.ReadAllTextAsync(path);
        Assert.Contains("\"host\": \"Google\"", text);
        Assert.Contains("\"status\": \"Available\"", text);
    }

    private static HostSnapshot CreateSnapshot()
    {
        return new HostSnapshot
        {
            Host = new Host { Name = "Google", Address = "google.com" },
            Status = HostStatus.Available,
            Latency = TimeSpan.FromMilliseconds(25),
            PacketLoss = 0,
            Timestamp = DateTimeOffset.Now
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(m_directory))
        {
            Directory.Delete(m_directory, true);
        }
    }
}
