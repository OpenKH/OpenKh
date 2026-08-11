using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class PresetsWindow : Window
{
    private readonly PresetService? _presets;
    private readonly IReadOnlyCollection<string> _enabledModIds = [];

    public PresetsWindow()
    {
        InitializeComponent();
    }

    public PresetsWindow(PresetService presets, IReadOnlyCollection<string> enabledModIds) : this()
    {
        _presets = presets;
        _enabledModIds = enabledModIds;
        RefreshNames();
    }

    private void RefreshNames()
    {
        if (_presets is null)
            return;
        PresetList.ItemsSource = _presets.GetNames();
        PresetList.SelectedIndex = PresetList.ItemCount > 0 ? 0 : -1;
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        var selected = PresetList.SelectedItem is string;
        ApplyButton.IsEnabled = selected;
        RemoveButton.IsEnabled = selected;
    }

    private void PresetList_OnSelectionChanged(object? sender, SelectionChangedEventArgs eventArgs) =>
        RefreshButtons();

    private void SavePreset_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_presets is null)
            return;
        try
        {
            _presets.Save(PresetNameTextBox.Text ?? "", _enabledModIds);
            PresetNameTextBox.Text = "";
            RefreshNames();
        }
        catch
        {
            PresetNameTextBox.Focus();
        }
    }

    private void Apply_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_presets is not null && PresetList.SelectedItem is string name)
            Close(_presets.Load(name));
    }

    private void Remove_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_presets is null || PresetList.SelectedItem is not string name)
            return;
        _presets.Remove(name);
        RefreshNames();
    }

    private void Close_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(null);

    public void HandleControllerAction(ControllerAction action)
    {
        if (action is ControllerAction.PreviousControl)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action is ControllerAction.PreviousItem)
            PresetList.SelectedIndex = Math.Max(0, PresetList.SelectedIndex - 1);
        else if (action is ControllerAction.NextItem)
            PresetList.SelectedIndex = Math.Min(PresetList.ItemCount - 1, PresetList.SelectedIndex + 1);
        else if (action == ControllerAction.Cancel)
            Close(null);
        else if (action == ControllerAction.Confirm)
            Apply_OnClick(ApplyButton, new RoutedEventArgs());
        RefreshButtons();
    }
}
