using System.Windows;

namespace LiquidPing.App.Services;

public sealed class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string message)
    {
        var result = MessageBox.Show(
            message,
            "Liquid Ping",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task ShowError(string message)
    {
        MessageBox.Show(message, "Liquid Ping", MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }
}
