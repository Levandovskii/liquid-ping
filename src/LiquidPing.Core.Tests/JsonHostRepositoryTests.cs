using LiquidPing.Core.Models;
using LiquidPing.Core.Storage;

namespace LiquidPing.Core.Tests;

public sealed class JsonHostRepositoryTests : IDisposable
{
    private readonly string m_directory = Path.Combine(Path.GetTempPath(), "liquidping-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task LoadAsync_MissingFile_ReturnsSixDefaultHosts()
    {
        var repository = CreateRepository();

        var hosts = await repository.LoadAsync(CancellationToken.None);

        Assert.Equal(6, hosts.Count);
        Assert.Contains(hosts, host => host.Name == "Google");
        Assert.Contains(hosts, host => host.Name == "Steam");
    }

    [Fact]
    public async Task SaveAndLoad_KeepsHostsAndSettings()
    {
        var repository = CreateRepository();
        var hosts = new[]
        {
            new Host { Name = "Test", Address = "example.com", TcpPort = 8443 }
        };

        await repository.SaveAsync(hosts, CancellationToken.None);
        var restored = await repository.LoadAsync(CancellationToken.None);

        Assert.Single(restored);
        Assert.Equal("Test", restored[0].Name);
        Assert.Equal(8443, restored[0].TcpPort);
    }

    [Fact]
    public async Task SettingsStore_DefaultsAndRoundTrip()
    {
        var store = new JsonSettingsStore(new JsonFileStore(), Path.Combine(m_directory, "settings.json"));

        var defaults = await store.LoadAsync(CancellationToken.None);
        Assert.Equal(5, defaults.IntervalSeconds);
        Assert.Equal(10, defaults.ChartWindowMinutes);

        defaults.IntervalSeconds = 15;
        defaults.Theme = ThemeMode.Dark;
        await store.SaveAsync(defaults, CancellationToken.None);

        var restored = await store.LoadAsync(CancellationToken.None);
        Assert.Equal(15, restored.IntervalSeconds);
        Assert.Equal(ThemeMode.Dark, restored.Theme);
    }

    private JsonHostRepository CreateRepository()
    {
        Directory.CreateDirectory(m_directory);
        return new JsonHostRepository(new JsonFileStore(), Path.Combine(m_directory, "hosts.json"));
    }

    public void Dispose()
    {
        if (Directory.Exists(m_directory))
        {
            Directory.Delete(m_directory, true);
        }
    }
}
