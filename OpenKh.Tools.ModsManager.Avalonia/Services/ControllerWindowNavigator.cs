using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.VisualTree;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public static class ControllerWindowNavigator
{
    private const double ScrollStep = 96;

    public static Action<ControllerAction> WithScrolling(
        Control root,
        Action<ControllerAction> handler) =>
        action =>
        {
            if (!TryHideVirtualKeyboard(action) &&
                !TryShowVirtualKeyboard(root, action) &&
                !TryScroll(root, action))
                handler(action);
        };

    public static bool TryHideVirtualKeyboard(ControllerAction action) =>
        action == ControllerAction.Cancel && VirtualKeyboardService.Hide();

    public static bool TryShowVirtualKeyboard(Control root, ControllerAction action)
    {
        if (action != ControllerAction.Confirm)
            return false;

        var focused = TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control;
        var textBox = focused as TextBox ?? focused?.GetVisualAncestors().OfType<TextBox>().FirstOrDefault();
        if (textBox is null || textBox.IsReadOnly)
            return false;

        VirtualKeyboardService.Show(textBox);
        return true;
    }

    public static bool TryScroll(Control root, ControllerAction action)
    {
        if (action is not (ControllerAction.ScrollUp or ControllerAction.ScrollDown))
            return false;

        var focused = TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control;
        var scrollViewer = focused?.GetVisualAncestors()
            .OfType<ScrollViewer>()
            .FirstOrDefault(CanScroll);
        scrollViewer ??= root.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .Where(CanScroll)
            .OrderByDescending(viewer => viewer.Bounds.Width * viewer.Bounds.Height)
            .FirstOrDefault();
        if (scrollViewer is null)
            return true;

        var direction = action == ControllerAction.ScrollUp ? -1 : 1;
        var maximum = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        var nextOffset = Math.Clamp(scrollViewer.Offset.Y + (direction * ScrollStep), 0, maximum);
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, nextOffset);
        return true;
    }

    public static void MoveFocus(Control root, int offset)
    {
        var controls = root.GetVisualDescendants()
            .OfType<Control>()
            .Where(IsAvailable)
            .ToArray();
        if (controls.Length == 0)
            return;

        var focused = TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control;
        var currentIndex = focused is null ? -1 : Array.IndexOf(controls, focused);
        if (currentIndex < 0)
            currentIndex = offset > 0 ? -1 : 0;

        for (var step = 1; step <= controls.Length; step++)
        {
            var nextIndex = (currentIndex + (offset * step) + controls.Length) % controls.Length;
            if (controls[nextIndex].Focus(NavigationMethod.Directional))
                return;
        }
    }

    private static bool IsAvailable(Control control) =>
        control.Focusable &&
        control.IsEffectivelyEnabled &&
        control.IsVisible &&
        control.GetVisualAncestors()
            .OfType<Control>()
            .All(ancestor => ancestor.IsVisible && ancestor.IsEffectivelyEnabled);

    private static bool CanScroll(ScrollViewer viewer) =>
        viewer.IsVisible &&
        viewer.IsEffectivelyEnabled &&
        viewer.Extent.Height > viewer.Viewport.Height + 1 &&
        viewer.GetVisualAncestors()
            .OfType<Control>()
            .All(ancestor => ancestor.IsVisible && ancestor.IsEffectivelyEnabled);
}
