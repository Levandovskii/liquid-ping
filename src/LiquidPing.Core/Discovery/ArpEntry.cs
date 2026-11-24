using System.Net;
using System.Net.NetworkInformation;

namespace LiquidPing.Core.Discovery;

public sealed record ArpEntry(IPAddress Address, PhysicalAddress MacAddress);
