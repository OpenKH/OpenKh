using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaCollectionSettingsPrompt(
    MainWindow owner,
    CollectionSettingsService settings,
    IControllerInputService controller) : ICollectionSettingsPrompt
{
    public async Task<bool> ShowAsync(ModEntry mod, GameInfo game)
    {
        var window = new CollectionSettingsWindow(settings, mod, game);
        using var capture = controller.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        return await owner.ShowPageAsync<bool>(window);
    }
}
