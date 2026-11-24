using System.Net;
using LiquidPing.App.ViewModels;
using LiquidPing.Core.Models;

namespace LiquidPing.App.ViewModels;

public sealed class HostCardViewModel : ViewModelBase
{
    private HostStatus m_status = HostStatus.Pending;
    private TimeSpan? m_latency;
    private double m_packetLoss;
    private IPAddress? m_resolvedAddress;

    public HostCardViewModel(Host host)
    {
        Host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public Host Host { get; }

    public string Name => Host.Name;

    public string Address => Host.Address;

    public HostStatus Status
    {
        get => m_status;
        private set => SetProperty(ref m_status, value);
    }

    public TimeSpan? Latency
    {
        get => m_latency;
        private set
        {
            if (SetProperty(ref m_latency, value))
            {
                OnPropertyChanged(nameof(LatencyText));
            }
        }
    }

    public string LatencyText => m_latency is null ? "—" : $"{m_latency.Value.TotalMilliseconds:F0} мс";

    public double PacketLoss
    {
        get => m_packetLoss;
        private set
        {
            if (SetProperty(ref m_packetLoss, value))
            {
                OnPropertyChanged(nameof(PacketLossText));
            }
        }
    }

    public string PacketLossText => $"{m_packetLoss:F1} %";

    public IPAddress? ResolvedAddress
    {
        get => m_resolvedAddress;
        private set
        {
            if (SetProperty(ref m_resolvedAddress, value))
            {
                OnPropertyChanged(nameof(ResolvedAddressText));
            }
        }
    }

    public string ResolvedAddressText => m_resolvedAddress?.ToString() ?? "—";

    public void Apply(HostSnapshot snapshot)
    {
        Status = snapshot.Status;
        Latency = snapshot.Latency;
        PacketLoss = snapshot.PacketLoss;
        ResolvedAddress = snapshot.ResolvedAddress;
    }
}
