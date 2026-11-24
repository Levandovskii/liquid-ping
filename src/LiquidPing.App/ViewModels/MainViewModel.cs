using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using LiquidPing.App.Services;
using LiquidPing.Core.Models;
using LiquidPing.Core.Monitoring;
using LiquidPing.Core.Storage;

namespace LiquidPing.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IMonitoringService m_monitoring;
    private readonly IHostRepository m_repository;
    private readonly ISettingsStore m_settingsStore;
    private readonly MonitoringCoordinator m_coordinator;
    private readonly IDialogService m_dialogs;
    private AppSettings m_settings = new();
    private bool m_isMonitoring;
    private HostCardViewModel? m_selectedHost;

    public MainViewModel(
        IMonitoringService monitoring,
        IHostRepository repository,
        ISettingsStore settingsStore,
        MonitoringCoordinator coordinator,
        IDialogService dialogs)
    {
        m_monitoring = monitoring ?? throw new ArgumentNullException(nameof(monitoring));
        m_repository = repository ?? throw new ArgumentNullException(nameof(repository));
        m_settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        m_coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        m_dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        m_coordinator.BatchReady += OnBatchReady;

        CheckCommand = new AsyncRelayCommand(CheckAsync);
        ToggleMonitoringCommand = new RelayCommand(ToggleMonitoring);
        AddHostCommand = new AsyncRelayCommand(AddHostAsync);
        RemoveHostCommand = new AsyncRelayCommand(RemoveHostAsync);
    }

    public ObservableCollection<HostCardViewModel> Hosts { get; } = new();

    public IAsyncRelayCommand CheckCommand { get; }

    public IRelayCommand ToggleMonitoringCommand { get; }

    public IAsyncRelayCommand AddHostCommand { get; }

    public IAsyncRelayCommand RemoveHostCommand { get; }

    public bool IsMonitoring
    {
        get => m_isMonitoring;
        private set
        {
            if (SetProperty(ref m_isMonitoring, value))
            {
                OnPropertyChanged(nameof(MonitoringButtonText));
            }
        }
    }

    public string MonitoringButtonText => IsMonitoring ? "Стоп" : "Старт";

    public HostCardViewModel? SelectedHost
    {
        get => m_selectedHost;
        set => SetProperty(ref m_selectedHost, value);
    }

    public async Task LoadAsync()
    {
        m_settings = await m_settingsStore.LoadAsync(CancellationToken.None);
        var hosts = await m_repository.LoadAsync(CancellationToken.None);

        Hosts.Clear();

        foreach (var host in hosts)
        {
            Hosts.Add(new HostCardViewModel(host));
        }

        m_monitoring.UpdateHosts(hosts);
    }

    private async Task CheckAsync()
    {
        await m_monitoring.CheckOnceAsync(CancellationToken.None);
    }

    private void ToggleMonitoring()
    {
        if (IsMonitoring)
        {
            m_coordinator.Stop();
            IsMonitoring = false;
            return;
        }

        m_coordinator.Start(TimeSpan.FromSeconds(m_settings.IntervalSeconds));
        IsMonitoring = true;
    }

    private async Task AddHostAsync()
    {
        var host = new Host
        {
            Name = "Новый узел",
            Address = "example.com",
            HttpUrl = "https://example.com/"
        };

        if (!host.IsValid())
        {
            await m_dialogs.ShowError("Некорректные данные узла.");
            return;
        }

        Hosts.Add(new HostCardViewModel(host));
        await PersistAsync();
    }

    private async Task RemoveHostAsync()
    {
        if (SelectedHost is null)
        {
            return;
        }

        if (!await m_dialogs.ConfirmAsync($"Удалить узел {SelectedHost.Name}?"))
        {
            return;
        }

        Hosts.Remove(SelectedHost);
        SelectedHost = null;
        await PersistAsync();
    }

    private async Task PersistAsync()
    {
        var hosts = Hosts.Select(card => card.Host).ToArray();
        m_monitoring.UpdateHosts(hosts);
        await m_repository.SaveAsync(hosts, CancellationToken.None);
    }

    private void OnBatchReady(object? sender, BatchEventArgs args)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher is null)
        {
            ApplyBatch(args.Snapshots);
            return;
        }

        dispatcher.Invoke(() => ApplyBatch(args.Snapshots));
    }

    private void ApplyBatch(IReadOnlyList<HostSnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            var card = Hosts.FirstOrDefault(item => item.Host.Id == snapshot.Host.Id);
            card?.Apply(snapshot);
        }
    }
}
