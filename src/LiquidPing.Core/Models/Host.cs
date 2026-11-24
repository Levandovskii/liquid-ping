using System.Net;

namespace LiquidPing.Core.Models;

public sealed class Host
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int TcpPort { get; set; } = 443;
    public string HttpUrl { get; set; } = string.Empty;
    public bool UseIcmp { get; set; } = true;
    public bool UseTcp { get; set; } = true;
    public bool UseHttp { get; set; } = true;

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
        {
            return false;
        }

        if (TcpPort is < 1 or > 65535)
        {
            return false;
        }

        if (!UseIcmp && !UseTcp && !UseHttp)
        {
            return false;
        }

        if (UseHttp && !string.IsNullOrWhiteSpace(HttpUrl))
        {
            if (!Uri.TryCreate(HttpUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }
        }

        if (IPAddress.TryParse(Address, out _))
        {
            return true;
        }

        return Uri.CheckHostName(Address) == UriHostNameType.Dns;
    }
}
