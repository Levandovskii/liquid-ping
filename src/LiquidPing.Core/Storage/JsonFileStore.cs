using System.Text.Json;

namespace LiquidPing.Core.Storage;

public sealed class JsonFileStore
{
    private static readonly JsonSerializerOptions OPTIONS = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<T?> ReadAsync<T>(string path, CancellationToken ct)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer
                .DeserializeAsync<T>(stream, OPTIONS, ct)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            Backup(path);
            return default;
        }
    }

    public async Task WriteAsync<T>(string path, T value, CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporary = path + ".tmp";

        await using (var stream = File.Create(temporary))
        {
            await JsonSerializer.SerializeAsync(stream, value, OPTIONS, ct).ConfigureAwait(false);
        }

        File.Move(temporary, path, true);
    }

    private static void Backup(string path)
    {
        try
        {
            File.Move(path, path + ".bak", true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
