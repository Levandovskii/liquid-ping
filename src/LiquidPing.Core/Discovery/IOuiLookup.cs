using System.Net.NetworkInformation;

namespace LiquidPing.Core.Discovery;

public interface IOuiLookup
{
    string? ResolveVendor(PhysicalAddress macAddress);
}
