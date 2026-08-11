using Avalonia.Controls;
using Avalonia.VisualTree;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public static class ControllerWindowNavigator
{
    public static void MoveFocus(Window window, int offset)
    {
        var controls = window.GetVisualDescendants()
            .OfType<Control>()
            .Where(control => control.Focusable && control.IsEffectivelyEnabled && control.IsVisible)
            .ToArray();
        if (controls.Length == 0)
            return;

        var focused = window.FocusManager?.GetFocusedElement() as Control;
        var currentIndex = focused is null ? -1 : Array.IndexOf(controls, focused);
        var nextIndex = (currentIndex + offset + controls.Length) % controls.Length;
        controls[nextIndex].Focus();
    }
}
