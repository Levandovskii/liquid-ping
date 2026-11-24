namespace LiquidPing.Core.Storage;

public static class AppPaths
{
    public static string DataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LiquidPing");

    public static string HostsFile => Path.Combine(DataDirectory, "hosts.json");

    public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");

    public static string LogDirectory => Path.Combine(DataDirectory, "logs");
}
