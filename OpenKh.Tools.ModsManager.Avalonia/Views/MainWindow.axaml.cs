using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Avalonia.ViewModels;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        VirtualKeyboardService.Configure(new EmbeddedVirtualKeyboardHost(this));
        SizeChanged += (_, _) => UpdateResponsiveLayout();
        Opened += (_, _) => UpdateResponsiveLayout();
    }

    public async Task<T?> ShowPageAsync<T>(EmbeddedDialogControl page)
    {
        var previousPage = DialogContent.Content;
        DialogContent.Content = page;
        DialogOverlay.IsVisible = true;

        try
        {
            var result = await page.ShowEmbeddedAsync();
            return result is T value ? value : default;
        }
        finally
        {
            if (ReferenceEquals(DialogContent.Content, page))
            {
                DialogContent.Content = previousPage;
                DialogOverlay.IsVisible = previousPage is not null;
            }
        }
    }

    private void UpdateResponsiveLayout()
    {
        var compact = Bounds.Width < 1120;
        WorkspaceGrid.ColumnDefinitions.Clear();
        WorkspaceGrid.RowDefinitions.Clear();
        if (compact)
        {
            WorkspaceScroller.VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
            WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            WorkspaceGrid.RowDefinitions.Add(new RowDefinition(new GridLength(3, GridUnitType.Star)));
            WorkspaceGrid.RowDefinitions.Add(new RowDefinition(new GridLength(18)));
            WorkspaceGrid.RowDefinitions.Add(new RowDefinition(new GridLength(2, GridUnitType.Star)));
            Grid.SetColumn(ModDetailsPanel, 0);
            Grid.SetRow(ModDetailsPanel, 2);
            WorkspaceGrid.MinHeight = 860;
        }
        else
        {
            WorkspaceScroller.VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
            WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));
            WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(18)));
            WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
            WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            Grid.SetColumn(ModDetailsPanel, 2);
            Grid.SetRow(ModDetailsPanel, 0);
            WorkspaceGrid.MinHeight = 0;
        }
    }

    private void ModList_OnSelectionChanged(object? sender, SelectionChangedEventArgs eventArgs)
    {
        if (sender is not ListBox listBox || listBox.SelectedItem is not { } selectedItem)
            return;

        // Wait for collection and virtualization updates before bringing a moved item back into view.
        Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(listBox.SelectedItem, selectedItem))
                listBox.ScrollIntoView(selectedItem);
        }, DispatcherPriority.Loaded);
    }

    private void ModList_OnGotFocus(object? sender, FocusChangedEventArgs eventArgs) =>
        SelectFocusedMod(eventArgs.Source as Control);

    public void HandleControllerAction(ControllerAction action)
    {
        if (ControllerWindowNavigator.TryHandleVirtualKeyboard(action) ||
            ControllerWindowNavigator.TryShowVirtualKeyboard(this, action) ||
            ControllerWindowNavigator.TryScroll(this, action))
            return;

        if (ControllerWindowNavigator.TryMoveFocus(this, action))
        {
            SelectFocusedMod(FocusManager?.GetFocusedElement() as Control);
            return;
        }

        if (action == ControllerAction.Confirm)
        {
            SelectFocusedMod(FocusManager?.GetFocusedElement() as Control);
            if (ActivateFocusedControl())
                return;
        }

        var restoreModFocus = false;
        if (action is ControllerAction.MoveUp or ControllerAction.MoveDown or ControllerAction.MoveTop)
        {
            var focusedControl = FocusManager?.GetFocusedElement() as Control;
            if (!TryGetFocusedMod(focusedControl, out var focusedMod))
                return;

            ModList.SelectedItem = focusedMod;
            restoreModFocus = true;
        }

        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.HandleControllerAction(action);
            if (restoreModFocus && viewModel.SelectedMod is { } selectedMod)
                RestoreModFocus(ModList, selectedMod);
        }
    }

    private static void RestoreModFocus(ListBox list, ModListItemViewModel selectedMod)
    {
        list.ScrollIntoView(selectedMod);
        Dispatcher.UIThread.Post(() =>
        {
            var item = list.GetVisualDescendants()
                .OfType<ListBoxItem>()
                .FirstOrDefault(candidate => ReferenceEquals(candidate.DataContext, selectedMod));
            item?.Focus(NavigationMethod.Directional);
        }, DispatcherPriority.Loaded);
    }

    private bool ActivateFocusedControl()
    {
        var focused = FocusManager?.GetFocusedElement();
        if (focused is Button button && button.Command is { } command)
        {
            if (command.CanExecute(button.CommandParameter))
                command.Execute(button.CommandParameter);
            return true;
        }

        if (GetFocusedExpander(focused) is { } expander)
        {
            expander.IsExpanded = !expander.IsExpanded;
            return true;
        }

        if (focused is ToggleSwitch toggleSwitch)
        {
            toggleSwitch.IsChecked = toggleSwitch.IsChecked != true;
            return true;
        }

        if (focused is CheckBox checkBox)
        {
            checkBox.IsChecked = checkBox.IsChecked != true;
            return true;
        }

        if (focused is ComboBox comboBox)
        {
            comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
            return true;
        }

        return false;
    }

    private static Expander? GetFocusedExpander(object? focused)
    {
        if (focused is Expander expander)
            return expander;

        return focused is ToggleButton toggleButton
            ? toggleButton.GetVisualAncestors().OfType<Expander>().FirstOrDefault()
            : null;
    }

    private void SelectFocusedMod(Control? focusedControl)
    {
        if (TryGetFocusedMod(focusedControl, out var mod))
            ModList.SelectedItem = mod;
    }

    private static bool TryGetFocusedMod(Control? focusedControl, out ModListItemViewModel? mod)
    {
        var item = focusedControl as ListBoxItem ??
            focusedControl?.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        mod = item?.DataContext as ModListItemViewModel;
        return mod is not null;
    }
}
