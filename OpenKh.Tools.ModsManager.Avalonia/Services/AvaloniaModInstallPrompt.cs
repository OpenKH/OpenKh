using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaModInstallPrompt(
    MainWindow owner,
    IControllerInputService controller) : IModInstallPrompt
{
    public async Task<ModInstallRequest?> ShowAsync()
    {
        var window = new InstallModWindow();
        using var capture = controller.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        return await owner.ShowPageAsync<ModInstallRequest>(window);
    }
}
