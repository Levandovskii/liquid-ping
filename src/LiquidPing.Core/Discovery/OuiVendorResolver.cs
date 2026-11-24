using System.Net.NetworkInformation;

namespace LiquidPing.Core.Discovery;

public sealed class OuiVendorResolver : IOuiLookup
{
    private static readonly IReadOnlyDictionary<string, string> VENDORS = new Dictionary<string, string>
    {
        ["000C29"] = "VMware",
        ["005056"] = "VMware",
        ["080027"] = "VirtualBox",
        ["00155D"] = "Microsoft",
        ["001C42"] = "Parallels",
        ["00163E"] = "Xensource",
        ["525400"] = "QEMU",
        ["0003FF"] = "Microsoft",
        ["001DD8"] = "Microsoft",
        ["3C5282"] = "Intel",
        ["A4C494"] = "Intel",
        ["F859C1"] = "Intel",
        ["001B63"] = "Apple",
        ["3C0754"] = "Apple",
        ["F0DBF8"] = "Apple",
        ["001E52"] = "Apple",
        ["002637"] = "Samsung",
        ["34145F"] = "Samsung",
        ["E8508B"] = "Samsung",
        ["001A11"] = "Google",
        ["F4F5D8"] = "Google",
        ["0018FE"] = "HP",
        ["3C4A92"] = "HP",
        ["001B78"] = "HP",
        ["001E0B"] = "HP",
        ["0024BE"] = "Sony",
        ["0026B9"] = "Dell",
        ["B8CA3A"] = "Dell",
        ["001422"] = "Dell",
        ["001D09"] = "Dell",
        ["1866DA"] = "Dell",
        ["001A2B"] = "Cisco",
        ["0025B4"] = "Cisco",
        ["00000C"] = "Cisco",
        ["001B0C"] = "Cisco",
        ["0018F3"] = "Asustek",
        ["2C56DC"] = "Asustek",
        ["002354"] = "Asustek",
        ["001C4A"] = "MikroTik",
        ["4C5E0C"] = "MikroTik",
        ["74D435"] = "MikroTik",
        ["001D0F"] = "TP-Link",
        ["1027F5"] = "TP-Link",
        ["50C7BF"] = "TP-Link",
        ["A42BB0"] = "TP-Link",
        ["001E10"] = "Huawei",
        ["00259E"] = "Huawei",
        ["781DBA"] = "Huawei",
        ["001E58"] = "D-Link",
        ["002191"] = "D-Link",
        ["0C8268"] = "Ubiquiti",
        ["24A43C"] = "Ubiquiti",
        ["B827EB"] = "Raspberry Pi",
        ["DCA632"] = "Raspberry Pi",
        ["E45F01"] = "Raspberry Pi",
        ["00248C"] = "Sony",
        ["00127F"] = "Cisco-Linksys",
        ["001A70"] = "Cisco-Linksys",
        ["C0C1C0"] = "Cisco-Linksys"
    };

    public string? ResolveVendor(PhysicalAddress macAddress)
    {
        ArgumentNullException.ThrowIfNull(macAddress);
        var bytes = macAddress.GetAddressBytes();

        if (bytes.Length < 3)
        {
            return null;
        }

        var prefix = $"{bytes[0]:X2}{bytes[1]:X2}{bytes[2]:X2}";
        return VENDORS.TryGetValue(prefix, out var vendor) ? vendor : null;
    }
}
