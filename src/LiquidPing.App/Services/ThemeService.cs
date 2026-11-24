using LiquidPing.Core.Models;

namespace LiquidPing.App.Services;

public sealed class ThemeService
{
    public ThemeMode Current { get; private set; } = ThemeMode.System;

    public void Apply(ThemeMode theme)
    {
        Current = theme;
    }
}
