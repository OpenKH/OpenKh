using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaModInstallPrompt(
    Window owner,
    IControllerInputService controller) : IModInstallPrompt
{
    public async Task<ModInstallRequest?> ShowAsync()
    {
        var window = new InstallModWindow();
        using var capture = controller.Capture(window.HandleControllerAction);
        return await window.ShowDialog<ModInstallRequest?>(owner);
    }
}
