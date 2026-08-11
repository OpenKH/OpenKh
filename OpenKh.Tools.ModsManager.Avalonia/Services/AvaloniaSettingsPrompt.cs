using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaSettingsPrompt(
    MainWindow owner,
    ModManagerConfigurationService configuration,
    PanaceaService panacea,
    OpenKhUpdateCheckerService updateChecker,
    IControllerInputService controller) : ISettingsPrompt
{
    public async Task<bool> ShowAsync()
    {
        var window = new SettingsWindow(configuration, panacea, updateChecker);
        using var capture = controller.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        return await owner.ShowPageAsync<bool>(window);
    }
}
