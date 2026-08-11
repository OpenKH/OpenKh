using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class InstallModWindow : EmbeddedDialogControl
{
    public InstallModWindow()
    {
        InitializeComponent();
        Opened += (_, _) => SourceTextBox.Focus();
    }

    private async void ChooseFile_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose an OpenKH mod",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("OpenKH mods")
                {
                    Patterns = ["*.zip", "*.kh2pcpatch", "*.kh1pcpatch", "*.compcpatch", "*.bbspcpatch", "*.dddpcpatch", "*.lua"]
                }
            ]
        });

        if (files.Count > 0 && files[0].TryGetLocalPath() is { } fileName)
            SourceTextBox.Text = fileName;
    }

    private void Install_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        var source = SourceTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(source))
        {
            SourceTextBox.Focus();
            return;
        }

        Close(new ModInstallRequest(
            source,
            string.IsNullOrWhiteSpace(BranchTextBox.Text) ? null : BranchTextBox.Text.Trim(),
            OverwriteCheckBox.IsChecked == true));
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(null);

    public void HandleControllerAction(ControllerAction action)
    {
        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem or ControllerAction.PreviousGame)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem or ControllerAction.NextGame)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close(null);
        else if (action == ControllerAction.Install)
            Install_OnClick(InstallButton, new RoutedEventArgs());
        else if (action == ControllerAction.Secondary)
            ChooseFile_OnClick(ChooseFileButton, new RoutedEventArgs());
        else if (action == ControllerAction.Confirm)
            ActivateFocusedControl();
    }

    private void ActivateFocusedControl()
    {
        var focused = FocusManager?.GetFocusedElement();
        if (focused == InstallButton)
            Install_OnClick(InstallButton, new RoutedEventArgs());
        else if (focused == CancelButton)
            Close(null);
        else if (focused == ChooseFileButton)
            ChooseFile_OnClick(ChooseFileButton, new RoutedEventArgs());
        else if (focused == OverwriteCheckBox)
            OverwriteCheckBox.IsChecked = OverwriteCheckBox.IsChecked != true;
        else
            ControllerWindowNavigator.MoveFocus(this, 1);
    }
}
