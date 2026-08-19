using Avalonia;
using Avalonia.Controls;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Avalonia.ViewModels;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
        if (sender is ListBox listBox && listBox.SelectedItem is not null)
            listBox.ScrollIntoView(listBox.SelectedItem);
    }

    public void HandleControllerAction(ControllerAction action)
    {
        if (ControllerWindowNavigator.TryHideVirtualKeyboard(action) ||
            ControllerWindowNavigator.TryShowVirtualKeyboard(this, action) ||
            ControllerWindowNavigator.TryScroll(this, action))
            return;

        if (action == ControllerAction.PreviousControl)
        {
            ControllerWindowNavigator.MoveFocus(this, -1);
            return;
        }

        if (action == ControllerAction.NextControl)
        {
            ControllerWindowNavigator.MoveFocus(this, 1);
            return;
        }

        if (action == ControllerAction.Confirm && ActivateFocusedControl())
            return;

        if (DataContext is MainWindowViewModel viewModel)
            viewModel.HandleControllerAction(action);
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
}
