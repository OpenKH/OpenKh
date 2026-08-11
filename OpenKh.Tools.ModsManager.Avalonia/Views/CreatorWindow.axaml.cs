using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Core;
using System.Diagnostics;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class CreatorWindow : EmbeddedDialogControl
{
    private readonly ModCreatorService? _creator;
    private readonly IControllerInputService? _controller;

    public CreatorWindow()
    {
        InitializeComponent();
        GameComboBox.DataContext = GameInfo.SupportedGames;
    }

    public CreatorWindow(ModCreatorService creator, IControllerInputService controller) : this()
    {
        _creator = creator;
        _controller = controller;
        PreferenceComboBox.ItemsSource = creator.GetPreferences().Select(preference => preference.Label).ToArray();
        GameDataTextBox.Text = creator.DefaultGameDataPath;
    }

    private async void Browse_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose a mod folder",
            AllowMultiple = false
        });
        if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } path)
        {
            DirectoryTextBox.Text = path;
            LoadExistingMetadata(path);
        }
    }

    private async void BrowseGameData_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose the extracted GameData folder",
            AllowMultiple = false
        });
        if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } path)
            GameDataTextBox.Text = path;
    }

    private async void BrowseDiffTool_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose a diff tool executable",
            AllowMultiple = false
        });
        if (files.Count > 0 && files[0].TryGetLocalPath() is { } path)
            DiffToolTextBox.Text = path;
    }

    private void Create_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_creator is null || GameComboBox.SelectedItem is not GameInfo game)
            return;
        try
        {
            var path = _creator.Create(
                DirectoryTextBox.Text ?? "",
                TitleTextBox.Text ?? "",
                AuthorTextBox.Text ?? "",
                DescriptionTextBox.Text ?? "",
                game);
            StatusText.Text = $"Created {path}";
            StatusText.Foreground = global::Avalonia.Media.Brush.Parse("#62D6A7");
        }
        catch (Exception exception)
        {
            StatusText.Text = exception.Message;
            StatusText.Foreground = global::Avalonia.Media.Brush.Parse("#FF8B8B");
        }
    }

    private void Preview_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_creator is null || GameComboBox.SelectedItem is not GameInfo game)
            return;
        try
        {
            var previewPath = _creator.CreatePreview(
                DirectoryTextBox.Text ?? string.Empty,
                TitleTextBox.Text ?? string.Empty,
                AuthorTextBox.Text ?? string.Empty,
                DescriptionTextBox.Text ?? string.Empty,
                game);
            var diffTool = DiffToolTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(diffTool) && File.Exists(diffTool))
            {
                var currentPath = Path.Combine(DirectoryTextBox.Text!, "mod.yml");
                if (!File.Exists(currentPath))
                {
                    currentPath = Path.Combine(Path.GetTempPath(), $"openkh-empty-mod-{Guid.NewGuid():N}.yml");
                    File.WriteAllText(currentPath, string.Empty);
                }
                var startInfo = new ProcessStartInfo(diffTool) { UseShellExecute = false };
                startInfo.ArgumentList.Add(currentPath);
                startInfo.ArgumentList.Add(previewPath);
                Process.Start(startInfo);
            }
            else
            {
                Process.Start(new ProcessStartInfo(previewPath) { UseShellExecute = true });
            }
            SetStatus("The generated preview was opened. Use Generate or update to apply it.", true);
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message, false);
        }
    }

    private async void AppendFiles_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_creator is null)
            return;
        var window = new TargetFilesWindow(
            _creator,
            DirectoryTextBox.Text ?? string.Empty,
            GameDataTextBox.Text ?? string.Empty,
            TargetSearchTextBox.Text ?? string.Empty);
        if (TopLevel.GetTopLevel(this) is not MainWindow owner)
            return;
        using var capture = _controller?.Capture(
            ControllerWindowNavigator.WithScrolling(window, window.HandleControllerAction));
        var result = await owner.ShowPageAsync<bool>(window);
        if (result)
            SetStatus("Selected target files were copied and appended to mod.yml.", true);
    }

    private void SavePreference_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_creator is null)
            return;
        try
        {
            var label = PreferenceComboBox.Text ?? string.Empty;
            _creator.SavePreference(new CreatorPreference
            {
                Label = label,
                ModDirectory = DirectoryTextBox.Text ?? string.Empty,
                GameDataPath = GameDataTextBox.Text ?? string.Empty,
                DiffToolPath = DiffToolTextBox.Text ?? string.Empty
            });
            PreferenceComboBox.ItemsSource = _creator.GetPreferences().Select(preference => preference.Label).ToArray();
            PreferenceComboBox.Text = label;
            SetStatus($"Preference '{label}' was saved.", true);
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message, false);
        }
    }

    private void LoadPreference_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_creator is null)
            return;
        var label = PreferenceComboBox.Text ?? string.Empty;
        var preference = _creator.GetPreferences().LastOrDefault(item =>
            item.Label.Equals(label, StringComparison.OrdinalIgnoreCase));
        if (preference is null)
        {
            SetStatus("The selected preference was not found.", false);
            return;
        }
        DirectoryTextBox.Text = preference.ModDirectory;
        GameDataTextBox.Text = preference.GameDataPath;
        DiffToolTextBox.Text = preference.DiffToolPath;
        LoadExistingMetadata(preference.ModDirectory);
        SetStatus($"Preference '{preference.Label}' was loaded.", true);
    }

    private void LoadExistingMetadata(string directory)
    {
        if (_creator is null)
            return;
        var path = Path.Combine(directory, "mod.yml");
        if (!File.Exists(path))
            return;
        try
        {
            var metadata = _creator.ReadOrCreate(path);
            TitleTextBox.Text = metadata.Title;
            AuthorTextBox.Text = metadata.OriginalAuthor;
            DescriptionTextBox.Text = metadata.Description;
            var game = GameInfo.FromId(metadata.Game);
            GameComboBox.SelectedItem = GameInfo.SupportedGames.FirstOrDefault(item => item.Id == game.Id);
        }
        catch (Exception exception)
        {
            SetStatus($"Could not load the existing mod.yml: {exception.Message}", false);
        }
    }

    private void SetStatus(string message, bool successful)
    {
        StatusText.Text = message;
        StatusText.Foreground = global::Avalonia.Media.Brush.Parse(successful ? "#62D6A7" : "#FF8B8B");
    }

    private void Close_OnClick(object? sender, RoutedEventArgs eventArgs) => Close();

    public void HandleControllerAction(ControllerAction action)
    {
        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close();
        else if (action == ControllerAction.Confirm)
        {
            var focused = FocusManager?.GetFocusedElement();
            if (focused == CreateButton)
                Create_OnClick(CreateButton, new RoutedEventArgs());
            else if (focused == PreviewButton)
                Preview_OnClick(PreviewButton, new RoutedEventArgs());
            else if (focused == BrowseButton)
                Browse_OnClick(BrowseButton, new RoutedEventArgs());
            else if (focused == BrowseGameDataButton)
                BrowseGameData_OnClick(BrowseGameDataButton, new RoutedEventArgs());
            else if (focused == BrowseDiffToolButton)
                BrowseDiffTool_OnClick(BrowseDiffToolButton, new RoutedEventArgs());
            else if (focused == AppendFilesButton)
                AppendFiles_OnClick(AppendFilesButton, new RoutedEventArgs());
            else if (focused == SavePreferenceButton)
                SavePreference_OnClick(SavePreferenceButton, new RoutedEventArgs());
            else if (focused == LoadPreferenceButton)
                LoadPreference_OnClick(LoadPreferenceButton, new RoutedEventArgs());
            else if (focused == CloseButton)
                Close();
            else
                ControllerWindowNavigator.MoveFocus(this, 1);
        }
    }
}
