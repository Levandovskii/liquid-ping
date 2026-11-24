using LiquidPing.Core.Storage;

namespace LiquidPing.Core.Tests;

public sealed class JsonHostRepositoryTestsExtra : IDisposable
{
    private readonly string m_directory = Path.Combine(Path.GetTempPath(), "liquidping-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SaveEmptyList_LoadReturnsEmptyList()
    {
        Directory.CreateDirectory(m_directory);
        var repository = new JsonHostRepository(new JsonFileStore(), Path.Combine(m_directory, "hosts.json"));

        await repository.SaveAsync(Array.Empty<LiquidPing.Core.Models.Host>(), CancellationToken.None);
        var restored = await repository.LoadAsync(CancellationToken.None);

        Assert.Empty(restored);
    }

    public void Dispose()
    {
        if (Directory.Exists(m_directory))
        {
            Directory.Delete(m_directory, true);
        }
    }
}
