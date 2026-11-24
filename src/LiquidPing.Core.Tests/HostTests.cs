using LiquidPing.Core.Models;

namespace LiquidPing.Core.Tests;

public sealed class HostTests
{
    [Fact]
    public void IsValid_DnsAddress_ReturnsTrue()
    {
        var host = new Host { Name = "Google", Address = "google.com" };

        Assert.True(host.IsValid());
    }

    [Fact]
    public void IsValid_IpAddress_ReturnsTrue()
    {
        var host = new Host { Name = "Local", Address = "192.168.1.1" };

        Assert.True(host.IsValid());
    }

    [Fact]
    public void IsValid_NoProbes_ReturnsFalse()
    {
        var host = new Host
        {
            Name = "Silent",
            Address = "192.168.1.1",
            UseIcmp = false,
            UseTcp = false,
            UseHttp = false
        };

        Assert.False(host.IsValid());
    }

    [Fact]
    public void IsValid_InvalidPort_ReturnsFalse()
    {
        var host = new Host { Name = "Bad", Address = "google.com", TcpPort = 70000 };

        Assert.False(host.IsValid());
    }

    [Fact]
    public void IsValid_InvalidHttpUrl_ReturnsFalse()
    {
        var host = new Host { Name = "Bad", Address = "google.com", HttpUrl = "ftp://google.com" };

        Assert.False(host.IsValid());
    }

    [Fact]
    public void IsValid_EmptyName_ReturnsFalse()
    {
        var host = new Host { Name = " ", Address = "google.com" };

        Assert.False(host.IsValid());
    }
}
