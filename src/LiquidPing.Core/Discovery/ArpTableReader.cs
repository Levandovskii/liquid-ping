using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace LiquidPing.Core.Discovery;

public sealed class ArpTableReader : IArpTableReader
{
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const int MAX_TABLE_SIZE = 16 * 1024 * 1024;
    private const int DYNAMIC_ENTRY = 3;
    private const int STATIC_ENTRY = 4;

    public IReadOnlyList<ArpEntry> Read()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Array.Empty<ArpEntry>();
        }

        var buffer = IntPtr.Zero;

        try
        {
            var size = 0;
            var result = GetIpNetTable(IntPtr.Zero, ref size, false);

            if (result != ERROR_INSUFFICIENT_BUFFER && result != 0)
            {
                return Array.Empty<ArpEntry>();
            }

            if (size <= 0 || size > MAX_TABLE_SIZE)
            {
                return Array.Empty<ArpEntry>();
            }

            buffer = Marshal.AllocHGlobal(size);
            result = GetIpNetTable(buffer, ref size, false);

            if (result != 0)
            {
                return Array.Empty<ArpEntry>();
            }

            return ParseTable(buffer);
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    private static IReadOnlyList<ArpEntry> ParseTable(IntPtr buffer)
    {
        var count = Marshal.ReadInt32(buffer);
        var entries = new List<ArpEntry>(count);
        var rowSize = Marshal.SizeOf<MibIpNetRow>();
        var rowPointer = IntPtr.Add(buffer, sizeof(int));

        for (var index = 0; index < count; index++)
        {
            var row = Marshal.PtrToStructure<MibIpNetRow>(rowPointer);

            if (row.PhysicalAddressLength is > 0 and <= 8 && row.Type is DYNAMIC_ENTRY or STATIC_ENTRY)
            {
                var bytes = new byte[row.PhysicalAddressLength];
                Array.Copy(row.PhysicalAddress, bytes, bytes.Length);
                var mac = new PhysicalAddress(bytes);
                var address = new IPAddress(row.Address);
                entries.Add(new ArpEntry(address, mac));
            }

            rowPointer = IntPtr.Add(rowPointer, rowSize);
        }

        return entries;
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetIpNetTable(IntPtr ipNetTable, ref int size, bool order);

    [StructLayout(LayoutKind.Sequential)]
    private struct MibIpNetRow
    {
        public int Index;
        public int PhysicalAddressLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] PhysicalAddress;

        public int Address;
        public int Type;
    }
}
