using LiquidPing.Core.Scanning;

namespace LiquidPing.Core.Tests;

public sealed class PortSetTests
{
    [Fact]
    public void FromRange_ReturnsAllPorts()
    {
        var set = PortSet.FromRange(80, 83);

        Assert.Equal(new[] { 80, 81, 82, 83 }, set.Ports);
        Assert.Equal("80-83", set.Label);
    }

    [Fact]
    public void Parse_SupportsListAndRanges()
    {
        var set = PortSet.Parse("22, 80, 8000-8002");

        Assert.Equal(new[] { 22, 80, 8000, 8001, 8002 }, set.Ports);
    }

    [Fact]
    public void Parse_RemovesDuplicates()
    {
        var set = PortSet.Parse("80,80,80-81");

        Assert.Equal(new[] { 80, 81 }, set.Ports);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("abc")]
    [InlineData("90-80")]
    [InlineData("")]
    public void Parse_InvalidInput_Throws(string text)
    {
        Assert.ThrowsAny<ArgumentException>(() => PortSet.Parse(text));
    }

    [Fact]
    public void Popular_ContainsHttps()
    {
        var set = PortSet.Popular();

        Assert.Contains(443, set.Ports);
        Assert.Contains(80, set.Ports);
    }
}
