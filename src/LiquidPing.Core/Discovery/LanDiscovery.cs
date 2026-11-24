using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using LiquidPing.Core.Models;
using LiquidPing.Core.Probes;

namespace LiquidPing.Core.Discovery;

public sealed class LanDiscovery : ILanDiscovery
{
    private const int MAX_ADDRESSES = 1024;
    private const int MIN_PREFIX_LENGTH = 22;
    private const int MAX_PARALLEL_PINGS = 64;
    private const int MAX_PARALLEL_NAMES = 32;
    private static readonly TimeSpan PING_TIMEOUT = TimeSpan.FromSeconds(1);

    private readonly IProbe m_probe;
    private readonly IArpTableReader m_arp;
    private readonly IOuiLookup m_oui;
    private readonly IHostNameResolver m_names;

    public LanDiscovery(
        IProbe probe,
        IArpTableReader arp,
        IOuiLookup oui,
        IHostNameResolver names)
    {
        m_probe = probe ?? throw new ArgumentNullException(nameof(probe));
        m_arp = arp ?? throw new ArgumentNullException(nameof(arp));
        m_oui = oui ?? throw new ArgumentNullException(nameof(oui));
        m_names = names ?? throw new ArgumentNullException(nameof(names));
    }

    public async Task<IReadOnlyList<LanHostInfo>> DiscoverAsync(
        IProgress<LanScanProgress>? progress,
        CancellationToken ct)
    {
        var subnets = CollectSubnets();
        var targets = subnets.SelectMany(subnet => subnet).Distinct().ToArray();
        var found = new ConcurrentDictionary<IPAddress, LanHostInfo>();
        var completed = 0;

        using (var semaphore = new SemaphoreSlim(MAX_PARALLEL_PINGS))
        {
            var tasks = targets.Select(async address =>
            {
                await semaphore.WaitAsync(ct).ConfigureAwait(false);

                try
                {
                    var result = await PingAsync(address, ct).ConfigureAwait(false);

                    if (result is not null)
                    {
                        found[address] = result;
                    }
                }
                finally
                {
                    semaphore.Release();
                    var current = Interlocked.Increment(ref completed);
                    progress?.Report(new LanScanProgress(current, targets.Length));
                }
            }).ToList();

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return found.Values.OrderBy(item => item.Address, IpAddressComparer.Instance).ToArray();
            }
        }

        MergeArpEntries(targets, found);
        await ResolveNamesAsync(found, ct).ConfigureAwait(false);
        return found.Values.OrderBy(item => item.Address, IpAddressComparer.Instance).ToArray();
    }

    private static IReadOnlyList<IReadOnlyList<IPAddress>> CollectSubnets()
    {
        var subnets = new List<IReadOnlyList<IPAddress>>();

        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (adapter.OperationalStatus != OperationalStatus.Up ||
                adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            foreach (var unicast in adapter.GetIPProperties().UnicastAddresses)
            {
                if (unicast.Address.AddressFamily != AddressFamily.InterNetwork ||
                    unicast.IPv4Mask is null)
                {
                    continue;
                }

                var prefix = CountPrefixBits(unicast.IPv4Mask);

                if (prefix < MIN_PREFIX_LENGTH)
                {
                    continue;
                }

                var addresses = EnumerateSubnet(unicast.Address, unicast.IPv4Mask);

                if (addresses.Count > MAX_ADDRESSES)
                {
                    continue;
                }

                subnets.Add(addresses);
            }
        }

        return subnets;
    }

    private static int CountPrefixBits(IPAddress mask)
    {
        var bits = 0;

        foreach (var value in mask.GetAddressBytes())
        {
            for (var bit = 7; bit >= 0; bit--)
            {
                if ((value & (1 << bit)) != 0)
                {
                    bits++;
                }
            }
        }

        return bits;
    }

    private static IReadOnlyList<IPAddress> EnumerateSubnet(IPAddress address, IPAddress mask)
    {
        var addressBytes = address.GetAddressBytes();
        var maskBytes = mask.GetAddressBytes();
        var network = new byte[addressBytes.Length];
        var broadcast = new byte[addressBytes.Length];

        for (var index = 0; index < addressBytes.Length; index++)
        {
            network[index] = (byte)(addressBytes[index] & maskBytes[index]);
            broadcast[index] = (byte)(network[index] | ~maskBytes[index]);
        }

        var first = ToUInt32(network) + 1;
        var last = ToUInt32(broadcast) - 1;
        var result = new List<IPAddress>();
        var prefix = CountPrefixBits(mask);

        if (prefix <= 30)
        {
            for (var value = first; value <= last; value++)
            {
                result.Add(FromUInt32(value));
            }
        }

        return result;
    }

    private static uint ToUInt32(byte[] bytes)
    {
        return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
    }

    private static IPAddress FromUInt32(uint value)
    {
        return new IPAddress(new[]
        {
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
        });
    }

    private async Task<LanHostInfo?> PingAsync(IPAddress address, CancellationToken ct)
    {
        var host = new Host
        {
            Id = Guid.NewGuid(),
            Name = address.ToString(),
            Address = address.ToString(),
            UseIcmp = true,
            UseTcp = false,
            UseHttp = false
        };

        var result = await m_probe.ProbeAsync(host, PING_TIMEOUT, ct).ConfigureAwait(false);

        if (!result.Success)
        {
            return null;
        }

        return new LanHostInfo(address, result.Latency, null, null, null);
    }

    private void MergeArpEntries(IReadOnlyList<IPAddress> targets, ConcurrentDictionary<IPAddress, LanHostInfo> found)
    {
        var targetSet = targets.ToHashSet();

        foreach (var entry in m_arp.Read())
        {
            if (!targetSet.Contains(entry.Address))
            {
                continue;
            }

            var vendor = m_oui.ResolveVendor(entry.MacAddress);
            found.AddOrUpdate(
                entry.Address,
                _ => new LanHostInfo(entry.Address, null, null, entry.MacAddress, vendor),
                (_, existing) => existing with { MacAddress = entry.MacAddress, Vendor = vendor });
        }
    }

    private async Task ResolveNamesAsync(ConcurrentDictionary<IPAddress, LanHostInfo> found, CancellationToken ct)
    {
        using var semaphore = new SemaphoreSlim(MAX_PARALLEL_NAMES);
        var tasks = found.Keys.Select(async address =>
        {
            await semaphore.WaitAsync(ct).ConfigureAwait(false);

            try
            {
                var name = await m_names.ResolveAsync(address, ct).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    found.AddOrUpdate(
                        address,
                        _ => new LanHostInfo(address, null, name, null, null),
                        (_, existing) => existing with { HostName = name });
                }
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
            }
            finally
            {
                semaphore.Release();
            }
        });

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
    }

    private sealed class IpAddressComparer : IComparer<IPAddress>
    {
        public static readonly IpAddressComparer Instance = new();

        public int Compare(IPAddress? x, IPAddress? y)
        {
            if (x is null || y is null)
            {
                return 0;
            }

            return ToUInt32(x.GetAddressBytes()).CompareTo(ToUInt32(y.GetAddressBytes()));
        }
    }
}
