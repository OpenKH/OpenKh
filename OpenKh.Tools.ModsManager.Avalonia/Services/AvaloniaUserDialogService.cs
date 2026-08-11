using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaUserDialogService(
    MainWindow owner,
    IControllerInputService controller) : IUserDialogService
{
    public async Task<bool> ConfirmAsync(string title, string message, string confirmText)
    {
        var window = new ConfirmationWindow(title, message, confirmText);
        using var capture = controller.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        return await owner.ShowPageAsync<bool>(window);
    }
}
