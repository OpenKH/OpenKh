using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaSetupPrompt(
    Window owner,
    ModManagerConfigurationService configuration,
    PanaceaService panacea,
    LuaBackendService luaBackend,
    GameExtractionService extraction,
    IControllerInputService controller) : ISetupPrompt
{
    public async Task<bool> ShowAsync()
    {
        var window = new SetupWindow(configuration, panacea, luaBackend, extraction);
        using var capture = controller.Capture(window.HandleControllerAction);
        return await window.ShowDialog<bool>(owner);
    }
}
