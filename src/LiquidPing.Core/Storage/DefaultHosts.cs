using LiquidPing.Core.Models;

namespace LiquidPing.Core.Storage;

public static class DefaultHosts
{
    public static IReadOnlyList<Host> Create()
    {
        return new[]
        {
            CreateHost("Яндекс", "yandex.ru"),
            CreateHost("Google", "google.com"),
            CreateHost("Steam", "store.steampowered.com"),
            CreateHost("Минсктранс", "minsktrans.by"),
            CreateHost("Ozon", "ozon.ru"),
            CreateHost("Wildberries", "wildberries.ru")
        };
    }

    private static Host CreateHost(string name, string address)
    {
        return new Host
        {
            Name = name,
            Address = address,
            TcpPort = 443,
            HttpUrl = $"https://{address}/",
            UseIcmp = true,
            UseTcp = true,
            UseHttp = true
        };
    }
}
