using System.Text.Json;
using LiquidPing.Core.Models;

namespace LiquidPing.Core.Support;

public sealed class JsonExporter : IResultExporter
{
    private static readonly JsonSerializerOptions OPTIONS = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task ExportAsync(
        string path,
        IReadOnlyList<HostSnapshot> snapshots,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var payload = snapshots.Select(snapshot => new
        {
            time = snapshot.Timestamp,
            host = snapshot.Host.Name,
            status = snapshot.Status.ToString(),
            latencyMs = snapshot.Latency?.TotalMilliseconds,
            packetLoss = snapshot.PacketLoss
        });

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, payload, OPTIONS, ct).ConfigureAwait(false);
    }
}
