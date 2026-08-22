using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
            if (!TryHandleVirtualKeyboard(action) &&
                !TryShowVirtualKeyboard(root, action) &&
                !TryScroll(root, action))
                handler(action);
        };

    public static bool TryHandleVirtualKeyboard(ControllerAction action)
    {
        if (!VirtualKeyboardService.IsOpen)
            return false;

        // Steam's keyboard overlay can leave the owner window active. Keep its
        // controller input from changing the UI behind the keyboard.
        if (action == ControllerAction.Cancel)
            VirtualKeyboardService.Hide();

        return true;
    }

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

    public static bool TryMoveFocus(Control root, ControllerAction action)
    {
        var direction = action switch
        {
            ControllerAction.NavigateUp => NavigationDirection.Up,
            ControllerAction.NavigateDown => NavigationDirection.Down,
            ControllerAction.NavigateLeft => NavigationDirection.Left,
            ControllerAction.NavigateRight => NavigationDirection.Right,
            _ => (NavigationDirection?)null
        };
        if (direction is null)
            return false;

        MoveFocus(root, direction.Value);
        return true;
    }

    public static void MoveFocus(Control root, NavigationDirection direction)
    {
        var focused = TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control;
        if (focused is null || !IsWithinRoot(focused, root))
        {
            focused = FindStartingTarget(root);
            if (focused is null || !TryFocusTarget(focused))
                return;

            SyncSelectionWithFocus(root);
        }

        var source = GetNavigationTarget(focused) ?? focused;
        var sourceBounds = GetBoundsRelativeTo(source, root);
        if (sourceBounds is null)
            return;

        var controls = root.GetVisualDescendants()
            .OfType<Control>()
            .Where(IsNavigationTarget)
            .Where(control => control != source)
            .Where(control => !IsNestedNavigationTarget(source, control))
            .Select(control => new
            {
                Control = control,
                Bounds = GetBoundsRelativeTo(control, root)
            })
            .Where(candidate => candidate.Bounds is not null)
            .ToArray();
        var bounds = controls.Select(candidate => candidate.Bounds!.Value).ToArray();
        var nextIndex = SpatialNavigation.FindNearest(sourceBounds.Value, bounds, direction);
        if (nextIndex < 0)
            return;

        var next = controls[nextIndex].Control;
        if (!TryFocusTarget(next))
            return;

        next.BringIntoView();
        SyncSelectionWithFocus(root);
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
            if (TryFocusTarget(controls[nextIndex]))
            {
                SyncSelectionWithFocus(root);
                return;
            }
        }
    }

    public static void SyncSelectionWithFocus(Control root)
    {
        var focused = TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control;
        var item = focused as ListBoxItem ??
            focused?.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        var listBox = item?.GetVisualAncestors().OfType<ListBox>().FirstOrDefault();
        if (item?.DataContext is not { } model || listBox is null)
            return;

        listBox.SelectedItem = model;
        listBox.ScrollIntoView(model);
    }

    private static bool IsAvailable(Control control) =>
        control.Focusable && IsVisibleAndEnabled(control);

    private static bool IsVisibleAndEnabled(Control control) =>
        control.IsEffectivelyEnabled &&
        control.IsVisible &&
        control.GetVisualAncestors()
            .OfType<Control>()
            .All(ancestor => ancestor.IsVisible && ancestor.IsEffectivelyEnabled);

    private static bool IsNavigationTarget(Control control) =>
        IsAvailable(control) &&
        IsNavigationTargetKind(control) &&
        !control.GetVisualAncestors()
            .OfType<Control>()
            .Any(ancestor => IsAvailable(ancestor) && IsNavigationTargetKind(ancestor));

    private static bool IsNavigationTargetKind(Control control) =>
        control is Button or TextBox or ComboBox or CheckBox or ToggleSwitch or Expander or ListBoxItem;

    private static Control? GetNavigationTarget(Control focused)
    {
        return focused.GetVisualAncestors()
            .Prepend(focused)
            .OfType<Control>()
            .Where(IsAvailable)
            .Where(IsNavigationTargetKind)
            .LastOrDefault();
    }

    private static Control? FindStartingTarget(Control root)
    {
        foreach (var listBox in root.GetVisualDescendants().OfType<ListBox>().Where(IsVisibleAndEnabled))
        {
            if (listBox.SelectedItem is not { } selected)
                continue;

            var selectedContainer = listBox.GetVisualDescendants()
                .OfType<ListBoxItem>()
                .FirstOrDefault(item => ReferenceEquals(item.DataContext, selected));
            if (selectedContainer is not null && IsNavigationTarget(selectedContainer))
                return selectedContainer;
        }

        return root.GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(IsNavigationTarget);
    }

    private static bool TryFocusTarget(Control target)
    {
        if (target.Focus(NavigationMethod.Directional))
            return true;

        return target.GetVisualDescendants()
            .OfType<Control>()
            .Where(IsAvailable)
            .Any(control => control.Focus(NavigationMethod.Directional));
    }

    private static bool IsWithinRoot(Control control, Control root) =>
        ReferenceEquals(control, root) || control.GetVisualAncestors().Contains(root);

    private static bool IsNestedNavigationTarget(Control source, Control candidate) =>
        candidate.GetVisualAncestors().Contains(source) ||
        source.GetVisualAncestors().Contains(candidate);

    private static Rect? GetBoundsRelativeTo(Control control, Control root)
    {
        var transform = control.TransformToVisual(root);
        var localBounds = new Rect(control.Bounds.Size);
        return transform is null ? null : localBounds.TransformToAABB(transform.Value);
    }

    private static bool CanScroll(ScrollViewer viewer) =>
        viewer.IsVisible &&
        viewer.IsEffectivelyEnabled &&
        viewer.Extent.Height > viewer.Viewport.Height + 1 &&
        viewer.GetVisualAncestors()
            .OfType<Control>()
            .All(ancestor => ancestor.IsVisible && ancestor.IsEffectivelyEnabled);
}
