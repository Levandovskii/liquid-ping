using System.Net;
using System.Net.NetworkInformation;

namespace LiquidPing.Core.Discovery;

public sealed record LanHostInfo(
    IPAddress Address,
    TimeSpan? Latency,
    string? HostName,
    PhysicalAddress? MacAddress,
    string? Vendor);
