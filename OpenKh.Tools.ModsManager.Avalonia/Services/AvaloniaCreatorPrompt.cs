using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaCreatorPrompt(
    MainWindow owner,
    ModCreatorService creator,
    IControllerInputService controller) : ICreatorPrompt
{
    public async Task ShowAsync()
    {
        var window = new CreatorWindow(creator, controller);
        using var capture = controller.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        await owner.ShowPageAsync<object>(window);
    }
}
