using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.ModsManager.Avalonia.ViewModels;
using OpenKh.Tools.ModsManager.Core;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class CollectionSettingsWindow : EmbeddedDialogControl
{
    private readonly CollectionSettingsService? _settings;
    private readonly ModEntry? _mod;
    private readonly GameInfo? _game;

    public CollectionSettingsWindow()
    {
        InitializeComponent();
    }

    public CollectionSettingsWindow(CollectionSettingsService settings, ModEntry mod, GameInfo game)
        : this()
    {
        _settings = settings;
        _mod = mod;
        _game = game;
        CollectionTitleText.Text = mod.Name;
        OptionsListBox.ItemsSource = settings.GetOptions(mod, game)
            .Select(option => new CollectionOptionViewModel(option))
            .ToArray();
    }

    private void Save_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_settings is not null && _mod is not null && _game is not null)
        {
            var options = (OptionsListBox.ItemsSource as IEnumerable<CollectionOptionViewModel>)
                ?.Select(option => option.ToModel()) ?? [];
            _settings.SaveOptions(_mod, _game, options);
        }
        Close(true);
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(false);

    public void HandleControllerAction(ControllerAction action)
    {
        if (ControllerWindowNavigator.TryMoveFocus(this, action))
            return;
        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem or ControllerAction.PreviousGame)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem or ControllerAction.NextGame)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close(false);
        else if (action == ControllerAction.Confirm)
        {
            var focused = FocusManager?.GetFocusedElement();
            if (focused == SaveButton)
                Save_OnClick(SaveButton, new RoutedEventArgs());
            else if (focused == CancelButton)
                Close(false);
            else if (focused is ToggleSwitch toggleSwitch)
                toggleSwitch.IsChecked = toggleSwitch.IsChecked != true;
            else
                ControllerWindowNavigator.MoveFocus(this, 1);
        }
    }
}
