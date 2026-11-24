namespace LiquidPing.App.Services;

public sealed class NavigationService
{
    public event EventHandler<string>? NavigationRequested;

    public void ShowDetails(Guid hostId)
    {
        NavigationRequested?.Invoke(this, $"details:{hostId}");
    }

    public void ShowPorts(Guid hostId)
    {
        NavigationRequested?.Invoke(this, $"ports:{hostId}");
    }

    public void ShowSettings()
    {
        NavigationRequested?.Invoke(this, "settings");
    }

    public void ShowLanDiscovery()
    {
        NavigationRequested?.Invoke(this, "lan");
    }
}
