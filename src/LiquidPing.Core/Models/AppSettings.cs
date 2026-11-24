using System.Globalization;

namespace LiquidPing.Core.Models;

public sealed class AppSettings
{
    public int IntervalSeconds { get; set; } = 5;
    public int IcmpTimeoutMs { get; set; } = 2000;
    public int TcpTimeoutMs { get; set; } = 1000;
    public int HttpTimeoutMs { get; set; } = 5000;
    public int PortTimeoutMs { get; set; } = 1000;
    public int ChartWindowMinutes { get; set; } = 10;
    public ThemeMode Theme { get; set; } = ThemeMode.System;
    public string Language { get; set; } = DefaultLanguage();
    public bool NotificationsEnabled { get; set; } = true;
    public bool TrayEnabled { get; set; }

    private static string DefaultLanguage()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "ru" : "en";
    }
}
