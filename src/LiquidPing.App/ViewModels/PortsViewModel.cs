using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LiquidPing.Core.Models;
using LiquidPing.Core.Scanning;

namespace LiquidPing.App.ViewModels;

public sealed class PortsViewModel : ViewModelBase
{
    private readonly IPortScanner m_scanner;
    private CancellationTokenSource? m_source;
    private Host? m_host;
    private PortSet m_portSet = PortSet.Popular();
    private int m_progress;

    public PortsViewModel(IPortScanner scanner)
    {
        m_scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        ScanCommand = new AsyncRelayCommand(ScanAsync);
        CancelCommand = new RelayCommand(Cancel);
    }

    public ObservableCollection<PortResult> Results { get; } = new();

    public IAsyncRelayCommand ScanCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public Host? Host
    {
        get => m_host;
        set => SetProperty(ref m_host, value);
    }

    public PortSet PortSet
    {
        get => m_portSet;
        set => SetProperty(ref m_portSet, value);
    }

    public int Progress
    {
        get => m_progress;
        private set => SetProperty(ref m_progress, value);
    }

    private async Task ScanAsync()
    {
        if (Host is null)
        {
            return;
        }

        Results.Clear();
        Progress = 0;
        m_source?.Dispose();
        m_source = new CancellationTokenSource();
        var progress = new Progress<PortScanProgress>(value =>
        {
            Progress = value.Total == 0 ? 0 : value.Completed * 100 / value.Total;
        });

        var result = await m_scanner.ScanAsync(Host, PortSet, progress, m_source.Token);

        foreach (var port in result.Results.Where(item => item.State == PortState.Open))
        {
            Results.Add(port);
        }
    }

    private void Cancel()
    {
        m_source?.Cancel();
    }
}
