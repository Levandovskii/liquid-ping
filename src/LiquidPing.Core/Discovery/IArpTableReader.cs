namespace LiquidPing.Core.Discovery;

public interface IArpTableReader
{
    IReadOnlyList<ArpEntry> Read();
}
