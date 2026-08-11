using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaSettingsPrompt(
    Window owner,
    ModManagerConfigurationService configuration,
    PanaceaService panacea,
    OpenKhUpdateCheckerService updateChecker,
    IControllerInputService controller) : ISettingsPrompt
{
    public async Task<bool> ShowAsync()
    {
        var window = new SettingsWindow(configuration, panacea, updateChecker);
        using var capture = controller.Capture(window.HandleControllerAction);
        return await window.ShowDialog<bool>(owner);
    }
}
