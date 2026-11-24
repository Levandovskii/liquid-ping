using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LiquidPing.Core.Models;
using LiquidPing.Core.Support;

namespace LiquidPing.App.ViewModels;

public sealed class HostDetailsViewModel : ViewModelBase
{
    private readonly IHistoryStore m_history;
    private Host? m_host;

    public HostDetailsViewModel(IHistoryStore history)
    {
        m_history = history ?? throw new ArgumentNullException(nameof(history));
        LoadHistoryCommand = new RelayCommand(LoadHistory);
    }

    public ObservableCollection<HistoryPoint> History { get; } = new();

    public IRelayCommand LoadHistoryCommand { get; }

    public Host? Host
    {
        get => m_host;
        set => SetProperty(ref m_host, value);
    }

    public void LoadHistory()
    {
        History.Clear();

        if (Host is null)
        {
            return;
        }

        foreach (var point in m_history.Get(Host.Id, TimeSpan.FromMinutes(10)))
        {
            History.Add(point);
        }
    }
}
