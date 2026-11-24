namespace LiquidPing.Core.Scanning;

public sealed class PortSet
{
    private static readonly int[] POPULAR_PORTS =
    {
        21, 22, 23, 25, 53, 80, 110, 135, 139, 143, 443, 445, 465, 587, 993, 995,
        1433, 1521, 3306, 3389, 5432, 5900, 6379, 8080, 8443, 9000
    };

    private PortSet(string label, IReadOnlyList<int> ports)
    {
        Label = label;
        Ports = ports;
    }

    public string Label { get; }

    public IReadOnlyList<int> Ports { get; }

    public static PortSet Popular()
    {
        return new PortSet("Популярные", POPULAR_PORTS);
    }

    public static PortSet FromRange(int from, int to)
    {
        ValidatePort(from);
        ValidatePort(to);

        if (from > to)
        {
            throw new ArgumentException("Начало диапазона больше конца.");
        }

        var ports = Enumerable.Range(from, to - from + 1).ToArray();
        return new PortSet($"{from}-{to}", ports);
    }

    public static PortSet Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Пустой список портов.", nameof(text));
        }

        var ports = new SortedSet<int>();

        foreach (var chunk in text.Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = chunk.IndexOf('-', StringComparison.Ordinal);

            if (separator < 0)
            {
                var port = ParsePort(chunk);
                ports.Add(port);
                continue;
            }

            var from = ParsePort(chunk[..separator]);
            var to = ParsePort(chunk[(separator + 1)..]);

            if (from > to)
            {
                throw new ArgumentException($"Некорректный диапазон: {chunk}.");
            }

            for (var port = from; port <= to; port++)
            {
                ports.Add(port);
            }
        }

        if (ports.Count == 0)
        {
            throw new ArgumentException("Пустой список портов.", nameof(text));
        }

        return new PortSet(text.Trim(), ports.ToArray());
    }

    private static int ParsePort(string value)
    {
        if (!int.TryParse(value.Trim(), out var port))
        {
            throw new ArgumentException($"Некорректный порт: {value}.");
        }

        ValidatePort(port);
        return port;
    }

    private static void ValidatePort(int port)
    {
        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Порт должен быть в диапазоне 1-65535.");
        }
    }
}
