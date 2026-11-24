using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LiquidPing.Core.Discovery;
using LiquidPing.Core.Models;
using LiquidPing.Core.Storage;

namespace LiquidPing.App.ViewModels;

public sealed class LanDiscoveryViewModel : ViewModelBase
{
    private readonly ILanDiscovery m_discovery;
    private readonly IHostRepository m_repository;
    private CancellationTokenSource? m_source;
    private bool m_isScanning;

    public LanDiscoveryViewModel(ILanDiscovery discovery, IHostRepository repository)
    {
        m_discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        m_repository = repository ?? throw new ArgumentNullException(nameof(repository));
        ScanCommand = new AsyncRelayCommand(ScanAsync);
        AddToMonitoringCommand = new AsyncRelayCommand<LanHostInfo>(AddToMonitoringAsync);
    }

    public ObservableCollection<LanHostInfo> Hosts { get; } = new();

    public IAsyncRelayCommand ScanCommand { get; }

    public IAsyncRelayCommand<LanHostInfo> AddToMonitoringCommand { get; }

    public bool IsScanning
    {
        get => m_isScanning;
        private set => SetProperty(ref m_isScanning, value);
    }

    private async Task ScanAsync()
    {
        if (IsScanning)
        {
            return;
        }

        IsScanning = true;
        Hosts.Clear();
        m_source?.Dispose();
        m_source = new CancellationTokenSource();

        try
        {
            var found = await m_discovery.DiscoverAsync(null, m_source.Token);

            foreach (var host in found)
            {
                Hosts.Add(host);
            }
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task AddToMonitoringAsync(LanHostInfo? info)
    {
        if (info is null)
        {
            return;
        }

        var hosts = await m_repository.LoadAsync(CancellationToken.None);

        if (hosts.Any(host => host.Address == info.Address.ToString()))
        {
            return;
        }

        var host = new Host
        {
            Name = info.HostName ?? info.Address.ToString(),
            Address = info.Address.ToString(),
            UseIcmp = true,
            UseTcp = false,
            UseHttp = false
        };

        var updated = hosts.Append(host).ToArray();
        await m_repository.SaveAsync(updated, CancellationToken.None);
    }
}
