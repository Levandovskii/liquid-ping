using System.Globalization;
using System.Text;
using LiquidPing.Core.Models;

namespace LiquidPing.Core.Support;

public sealed class CsvExporter : IResultExporter
{
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

        var builder = new StringBuilder();
        builder.AppendLine("time,host,status,latency_ms,packet_loss");

        foreach (var snapshot in snapshots)
        {
            var latency = snapshot.Latency is null
                ? string.Empty
                : snapshot.Latency.Value.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture);

            var line = string.Join(
                ',',
                snapshot.Timestamp.ToString("O", CultureInfo.InvariantCulture),
                Quote(snapshot.Host.Name),
                snapshot.Status,
                latency,
                snapshot.PacketLoss.ToString("F1", CultureInfo.InvariantCulture));

            builder.AppendLine(line);
        }

        await File.WriteAllTextAsync(path, builder.ToString(), ct).ConfigureAwait(false);
    }

    private static string Quote(string value)
    {
        return value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }
}
