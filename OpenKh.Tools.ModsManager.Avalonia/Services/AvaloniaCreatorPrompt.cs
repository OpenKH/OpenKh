using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaCreatorPrompt(
    Window owner,
    ModCreatorService creator,
    IControllerInputService controller) : ICreatorPrompt
{
    public async Task ShowAsync()
    {
        var window = new CreatorWindow(creator, controller);
        using var capture = controller.Capture(window.HandleControllerAction);
        await window.ShowDialog(owner);
    }
}
