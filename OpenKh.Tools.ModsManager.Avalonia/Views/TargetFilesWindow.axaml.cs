using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class TargetFilesWindow : EmbeddedDialogControl
{
    private readonly ModCreatorService? _creator;
    private readonly string _modDirectory = string.Empty;
    private readonly string _gameDataPath = string.Empty;

    public TargetFilesWindow()
    {
        InitializeComponent();
    }

    public TargetFilesWindow(
        ModCreatorService creator,
        string modDirectory,
        string gameDataPath,
        string searchText) : this()
    {
        _creator = creator;
        _modDirectory = modDirectory;
        _gameDataPath = gameDataPath;
        SearchTextBox.Text = searchText;
        Opened += async (_, _) => await SearchAsync();
    }

    private async void Search_OnClick(object? sender, RoutedEventArgs eventArgs) => await SearchAsync();

    private async Task SearchAsync()
    {
        if (_creator is null)
            return;
        SetBusy(true);
        StatusText.Text = "Searching extracted game data...";
        try
        {
            var files = await _creator.SearchFilesAsync(_gameDataPath, SearchTextBox.Text ?? string.Empty);
            FilesListBox.ItemsSource = files;
            StatusText.Text = files.Count == 1 ? "1 file found" : $"{files.Count} files found";
        }
        catch (Exception exception)
        {
            StatusText.Text = exception.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void Append_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_creator is null)
            return;
        var selected = FilesListBox.SelectedItems?.Cast<string>().ToArray() ?? [];
        if (selected.Length == 0)
        {
            StatusText.Text = "Select at least one file.";
            return;
        }
        try
        {
            _creator.AppendCopyFiles(_modDirectory, _gameDataPath, selected);
            Close(true);
        }
        catch (Exception exception)
        {
            StatusText.Text = exception.Message;
        }
    }

    private void SetBusy(bool busy)
    {
        SearchButton.IsEnabled = !busy;
        AppendButton.IsEnabled = !busy;
        FilesListBox.IsEnabled = !busy;
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(false);

    public void HandleControllerAction(ControllerAction action)
    {
        if (action is ControllerAction.PreviousControl)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action is ControllerAction.PreviousItem or ControllerAction.NextItem)
            MoveSelection(action == ControllerAction.PreviousItem ? -1 : 1);
        else if (action == ControllerAction.Cancel)
            Close(false);
        else if (action == ControllerAction.Confirm)
        {
            var focused = FocusManager?.GetFocusedElement();
            if (focused == AppendButton)
                Append_OnClick(AppendButton, new RoutedEventArgs());
            else if (focused == SearchButton)
                Search_OnClick(SearchButton, new RoutedEventArgs());
            else if (focused == CancelButton)
                Close(false);
            else
                ControllerWindowNavigator.MoveFocus(this, 1);
        }
    }

    private void MoveSelection(int offset)
    {
        if (FilesListBox.ItemCount == 0)
            return;
        FilesListBox.SelectedIndex = Math.Clamp(
            FilesListBox.SelectedIndex < 0 ? 0 : FilesListBox.SelectedIndex + offset,
            0,
            FilesListBox.ItemCount - 1);
        if (FilesListBox.SelectedItem is { } selected)
            FilesListBox.ScrollIntoView(selected);
    }
}
