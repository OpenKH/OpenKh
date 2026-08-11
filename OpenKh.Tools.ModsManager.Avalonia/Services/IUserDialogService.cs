namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IUserDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string confirmText);
}
