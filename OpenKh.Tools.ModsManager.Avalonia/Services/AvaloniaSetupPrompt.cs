using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaSetupPrompt(
    MainWindow owner,
    ModManagerConfigurationService configuration,
    PanaceaService panacea,
    LuaBackendService luaBackend,
    GameExtractionService extraction,
    IControllerInputService controller) : ISetupPrompt
{
    public async Task<bool> ShowAsync()
    {
        var window = new SetupWindow(configuration, panacea, luaBackend, extraction);
        using var capture = controller.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        return await owner.ShowPageAsync<bool>(window);
    }
}
