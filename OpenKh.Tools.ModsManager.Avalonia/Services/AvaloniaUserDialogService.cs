using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaUserDialogService(
    Window owner,
    IControllerInputService controller) : IUserDialogService
{
    public async Task<bool> ConfirmAsync(string title, string message, string confirmText)
    {
        var window = new ConfirmationWindow(title, message, confirmText);
        using var capture = controller.Capture(window.HandleControllerAction);
        return await window.ShowDialog<bool>(owner);
    }
}
