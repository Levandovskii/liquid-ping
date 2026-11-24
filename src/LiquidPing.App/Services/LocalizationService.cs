namespace LiquidPing.App.Services;

public sealed class LocalizationService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> STRINGS =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["ru"] = new Dictionary<string, string>
            {
                ["app.title"] = "Liquid Ping",
                ["action.check"] = "Проверить",
                ["action.start"] = "Старт",
                ["action.stop"] = "Стоп",
                ["action.add"] = "Добавить узел",
                ["status.pending"] = "Ожидание"
            },
            ["en"] = new Dictionary<string, string>
            {
                ["app.title"] = "Liquid Ping",
                ["action.check"] = "Check",
                ["action.start"] = "Start",
                ["action.stop"] = "Stop",
                ["action.add"] = "Add host",
                ["status.pending"] = "Pending"
            }
        };

    public string Language { get; private set; } = "ru";

    public void Apply(string language)
    {
        Language = STRINGS.ContainsKey(language) ? language : "ru";
    }

    public string Get(string key)
    {
        return STRINGS[Language].TryGetValue(key, out var value) ? value : key;
    }
}
