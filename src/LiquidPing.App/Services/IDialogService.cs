namespace LiquidPing.App.Services;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string message);

    Task ShowError(string message);
}
