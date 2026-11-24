namespace LiquidPing.App.Services;

public sealed class TrayService
{
    public bool IsEnabled { get; set; }

    public bool IsVisible { get; private set; }

    public void Show()
    {
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }
}
