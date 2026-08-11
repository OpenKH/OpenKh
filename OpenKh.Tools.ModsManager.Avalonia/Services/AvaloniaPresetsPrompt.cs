using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class AvaloniaPresetsPrompt(
    Window owner,
    PresetService presets,
    IControllerInputService controller) : IPresetsPrompt
{
    public async Task<IReadOnlyList<string>?> ShowAsync(IReadOnlyCollection<string> enabledModIds)
    {
        var window = new PresetsWindow(presets, enabledModIds);
        using var capture = controller.Capture(window.HandleControllerAction);
        return await window.ShowDialog<IReadOnlyList<string>?>(owner);
    }
}
