using System.Windows;
using LiquidPing.App.Services;
using LiquidPing.App.ViewModels;
using LiquidPing.Core.Discovery;
using LiquidPing.Core.Models;
using LiquidPing.Core.Monitoring;
using LiquidPing.Core.Probes;
using LiquidPing.Core.Scanning;
using LiquidPing.Core.Storage;
using LiquidPing.Core.Support;
using Microsoft.Extensions.DependencyInjection;

namespace LiquidPing.App;

public partial class App : Application
{
    private ServiceProvider? m_services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var collection = new ServiceCollection();
        ConfigureServices(collection);
        m_services = collection.BuildServiceProvider();
        var window = m_services.GetRequiredService<MainWindow>();
        window.DataContext = m_services.GetRequiredService<MainViewModel>();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        m_services?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<JsonFileStore>();
        services.AddSingleton<IHostRepository>(provider =>
            new JsonHostRepository(provider.GetRequiredService<JsonFileStore>()));
        services.AddSingleton<ISettingsStore>(provider =>
            new JsonSettingsStore(provider.GetRequiredService<JsonFileStore>()));

        services.AddSingleton<IProbe, IcmpProbe>();
        services.AddSingleton<IProbe, TcpProbe>();
        services.AddSingleton<IProbe>(_ => new HttpProbe());

        services.AddSingleton<StatusEvaluator>();
        services.AddSingleton<IMonitoringService, MonitoringService>();
        services.AddSingleton<IPortScanner, PortScanner>();
        services.AddSingleton<IArpTableReader, ArpTableReader>();
        services.AddSingleton<IOuiLookup, OuiVendorResolver>();
        services.AddSingleton<IHostNameResolver, DnsHostNameResolver>();
        services.AddSingleton<ILanDiscovery>(provider =>
            new LanDiscovery(
                provider.GetServices<IProbe>().First(probe => probe.Kind == ProbeKind.Icmp),
                provider.GetRequiredService<IArpTableReader>(),
                provider.GetRequiredService<IOuiLookup>(),
                provider.GetRequiredService<IHostNameResolver>()));
        services.AddSingleton<IHistoryStore, InMemoryHistoryStore>();
        services.AddSingleton<TrafficCounter>();

        services.AddSingleton<MonitoringCoordinator>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ThemeService>();
        services.AddSingleton<LocalizationService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<TrayService>();
        services.AddSingleton<NavigationService>();

        services.AddSingleton<MainViewModel>();
        services.AddTransient<HostDetailsViewModel>();
        services.AddTransient<PortsViewModel>();
        services.AddTransient<LanDiscoveryViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<MainWindow>();
    }
}
