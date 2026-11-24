using LiquidPing.Core.Storage;

namespace LiquidPing.Core.Tests;

public sealed class JsonFileStoreTests : IDisposable
{
    private readonly string m_directory = Path.Combine(Path.GetTempPath(), "liquidping-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task WriteAndRead_RoundTripsValue()
    {
        var store = new JsonFileStore();
        var path = Path.Combine(m_directory, "hosts.json");
        var value = new SampleDocument(1, new[] { "alpha", "beta" });

        await store.WriteAsync(path, value, CancellationToken.None);
        var restored = await store.ReadAsync<SampleDocument>(path, CancellationToken.None);

        Assert.NotNull(restored);
        Assert.Equal(1, restored.Version);
        Assert.Equal(new[] { "alpha", "beta" }, restored.Items);
    }

    [Fact]
    public async Task ReadAsync_MissingFile_ReturnsDefault()
    {
        var store = new JsonFileStore();
        var path = Path.Combine(m_directory, "missing.json");

        var restored = await store.ReadAsync<SampleDocument>(path, CancellationToken.None);

        Assert.Null(restored);
    }

    [Fact]
    public async Task ReadAsync_CorruptedFile_CreatesBackupAndReturnsDefault()
    {
        Directory.CreateDirectory(m_directory);
        var path = Path.Combine(m_directory, "broken.json");
        await File.WriteAllTextAsync(path, "{ broken", CancellationToken.None);
        var store = new JsonFileStore();

        var restored = await store.ReadAsync<SampleDocument>(path, CancellationToken.None);

        Assert.Null(restored);
        Assert.False(File.Exists(path));
        Assert.True(File.Exists(path + ".bak"));
    }

    public void Dispose()
    {
        if (Directory.Exists(m_directory))
        {
            Directory.Delete(m_directory, true);
        }
    }

    private sealed record SampleDocument(int Version, string[] Items);
}
