using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaInfoPrompt(Window owner, IControllerInputService controller) : IInfoPrompt
{
    public async Task ShowAsync()
    {
        var window = new InfoWindow();
        using var capture = controller.Capture(window.HandleControllerAction);
        await window.ShowDialog(owner);
    }
}
