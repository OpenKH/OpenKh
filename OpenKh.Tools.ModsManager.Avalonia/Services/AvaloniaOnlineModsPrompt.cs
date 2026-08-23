using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaOnlineModsPrompt(
    MainWindow owner,
    OnlineModCatalogService catalog,
    RepositoryModInstaller installer,
    IControllerInputService controller) : IOnlineModsPrompt
{
    public async Task<bool> ShowAsync(
        GameInfo game,
        IReadOnlyCollection<string> installedIds,
        Func<string, Task> onModInstalled)
    {
        var window = new OnlineModsWindow(catalog, installer, game, installedIds, onModInstalled);
        using var capture = controller.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        await owner.ShowPageAsync<bool>(window);
        return window.InstalledAny;
    }
}
