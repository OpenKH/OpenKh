using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaOnlineModsPrompt(
    Window owner,
    OnlineModCatalogService catalog,
    RepositoryModInstaller installer,
    IControllerInputService controller) : IOnlineModsPrompt
{
    public async Task<bool> ShowAsync(
        GameInfo game,
        IReadOnlyCollection<string> installedIds,
        Func<Task> onModInstalled)
    {
        var window = new OnlineModsWindow(catalog, installer, game, installedIds, onModInstalled);
        using var capture = controller.Capture(window.HandleControllerAction);
        await window.ShowDialog<bool>(owner);
        return window.InstalledAny;
    }
}
