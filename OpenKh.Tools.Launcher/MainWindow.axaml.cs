using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenKh.Tools.Launcher.Updates;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.Launcher;

public partial class MainWindow : Window
{
    private const string ApplicationsDirectory = "Apps";
    private const string FavoritesFileName = "launcher-favorites.txt";

    private static readonly IReadOnlyDictionary<string, string> ToolDescriptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["BarEditor"] = "Inspect and edit BAR archives.",
            ["BbsEventTableEditor"] = "Edit Birth by Sleep event tables.",
            ["BbsMapStudio"] = "Create and inspect Birth by Sleep maps.",
            ["ImageViewer"] = "View textures and supported game images.",
            ["IdxImg"] = "Browse and manage IDX/IMG game archives.",
            ["Kh2BattleEditor"] = "Edit Kingdom Hearts II battle data.",
            ["Kh2MapStudio"] = "Create and inspect Kingdom Hearts II maps.",
            ["Kh2MdlxEditor"] = "Inspect and edit Kingdom Hearts II models.",
            ["Kh2MsetEditor"] = "Inspect Kingdom Hearts II animation sets.",
            ["Kh2ObjectEditor"] = "Edit Kingdom Hearts II object data.",
            ["Kh2SystemEditor"] = "Edit Kingdom Hearts II system data.",
            ["Kh2TextEditor"] = "Edit game messages and text resources.",
            ["LayoutEditor"] = "Edit 2D layouts and interface assets.",
            ["MissionEditor"] = "Edit mission data.",
            ["ObjentryEditor"] = "Edit object entry tables.",
        };

    private readonly IControllerInputService _controller;
    private readonly List<ToolEntry> _allTools = new();
    private readonly HashSet<string> _favoriteToolNames = new(StringComparer.OrdinalIgnoreCase);
    private OpenKhReleaseUpdateCheckerService.CheckResult? _availableUpdate;
    private bool _compactCards;

    private string BaseDirectory => LauncherInstallation.RootDirectory;
    private string ApplicationsPath => Path.Combine(BaseDirectory, ApplicationsDirectory);
    private string FavoritesPath => Path.Combine(BaseDirectory, FavoritesFileName);
    private string ModManagerPath => LauncherInstallation.FindModManagerExecutable(BaseDirectory);
    private string CompatibilityModManagerPath => Path.Combine(BaseDirectory, "OpenKh.Tools.ModsManager.exe");

    public MainWindow() : this(new SdlControllerInputService())
    {
    }

    public MainWindow(IControllerInputService controller)
    {
        _controller = controller;
        InitializeComponent();
        Opened += HandleOpened;
        SizeChanged += HandleSizeChanged;
    }

    private void HandleOpened(object? sender, EventArgs eventArgs)
    {
        var processPath = Environment.ProcessPath;
        var version = string.IsNullOrWhiteSpace(processPath)
            ? null
            : FileVersionInfo.GetVersionInfo(processPath).ProductVersion;
        VersionText.Text = string.IsNullOrWhiteSpace(version) ? string.Empty : $"Version {version}";

        var modManagerAvailable = File.Exists(ModManagerPath);
        LaunchModManagerButton.IsEnabled = modManagerAvailable;
        CreateShortcutButton.IsEnabled = modManagerAvailable;
        ModManagerStatusText.Text = modManagerAvailable ? string.Empty : "Mod Manager was not found";
        ModManagerStatusText.IsVisible = !modManagerAvailable;

        LoadFavorites();
        LoadTools();
        UpdateCardLayout(Bounds.Width < 900);
        _ = RefreshUpdateAvailabilityAsync(showErrors: false, showProgress: false);
        LaunchModManagerButton.Focus();
    }

    private void HandleSizeChanged(object? sender, SizeChangedEventArgs eventArgs) =>
        UpdateCardLayout(eventArgs.NewSize.Width < 900);

    private void UpdateCardLayout(bool compact)
    {
        if (_compactCards == compact && HomeCardsGrid.ColumnDefinitions.Count > 0)
            return;

        _compactCards = compact;
        if (compact)
        {
            HomeCardsGrid.ColumnDefinitions = new ColumnDefinitions("*");
            HomeCardsGrid.RowDefinitions = new RowDefinitions("*,18,*");
            Grid.SetColumn(ModManagerCard, 0);
            Grid.SetRow(ModManagerCard, 0);
            Grid.SetColumn(ToolsCard, 0);
            Grid.SetRow(ToolsCard, 2);
        }
        else
        {
            HomeCardsGrid.ColumnDefinitions = new ColumnDefinitions("*,22,*");
            HomeCardsGrid.RowDefinitions = new RowDefinitions("*");
            Grid.SetColumn(ModManagerCard, 0);
            Grid.SetRow(ModManagerCard, 0);
            Grid.SetColumn(ToolsCard, 2);
            Grid.SetRow(ToolsCard, 0);
        }
    }

    private void LoadTools()
    {
        _allTools.Clear();
        if (Directory.Exists(ApplicationsPath))
        {
            _allTools.AddRange(Directory.EnumerateFiles(ApplicationsPath, "OpenKh.Tools.*", SearchOption.TopDirectoryOnly)
                .Where(IsApplicationExecutable)
                .Where(path => !Path.GetFileNameWithoutExtension(path)
                    .Equals("OpenKh.Tools.ModsManager", StringComparison.OrdinalIgnoreCase))
                .Select(CreateToolEntry));
        }

        ToolCountText.Text = _allTools.Count == 0
            ? "Tools are installed with the full OpenKH package"
            : $"{_allTools.Count} tools available";
        ApplyToolFilter();
    }

    private static bool IsApplicationExecutable(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(extension);
    }

    private ToolEntry CreateToolEntry(string executablePath)
    {
        var identifier = Path.GetFileName(executablePath);
        var fileName = Path.GetFileNameWithoutExtension(executablePath);
        var shortName = fileName.StartsWith("OpenKh.Tools.", StringComparison.OrdinalIgnoreCase)
            ? fileName["OpenKh.Tools.".Length..]
            : fileName;
        var displayName = HumanizeName(shortName);
        var description = ToolDescriptions.TryGetValue(shortName, out var knownDescription)
            ? knownDescription
            : "Open a specialized OpenKH modding utility.";
        return new ToolEntry(identifier, displayName, description, executablePath, _favoriteToolNames.Contains(identifier));
    }

    private static string HumanizeName(string value)
    {
        var result = Regex.Replace(value, "(?<=[a-z0-9])(?=[A-Z])", " ");
        return result
            .Replace("Kh1", "KH1", StringComparison.OrdinalIgnoreCase)
            .Replace("Kh2", "KH2", StringComparison.OrdinalIgnoreCase)
            .Replace("Bbs", "BBS", StringComparison.OrdinalIgnoreCase)
            .Replace("Idx", "IDX", StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyToolFilter()
    {
        var query = SearchBox?.Text?.Trim() ?? string.Empty;
        var matchingTools = string.IsNullOrWhiteSpace(query)
            ? _allTools.AsEnumerable()
            : _allTools.Where(tool => tool.DisplayName.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || tool.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase));
        var filteredTools = matchingTools
            .OrderByDescending(tool => tool.IsFavorite)
            .ThenBy(tool => tool.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (ToolsList is not null)
            ToolsList.ItemsSource = filteredTools;
        if (ToolsStatusText is not null)
        {
            ToolsStatusText.Text = !Directory.Exists(ApplicationsPath)
                ? "The Apps folder is not available in this installation."
                : $"Showing {filteredTools.Count} of {_allTools.Count} tools";
        }
    }

    private async void LaunchModManager_Click(object? sender, RoutedEventArgs eventArgs)
    {
        if (await TryLaunchAsync(ModManagerPath))
            Close();
    }

    private async void CheckForUpdates_Click(object? sender, RoutedEventArgs eventArgs)
    {
        CheckForUpdatesButton.IsEnabled = false;
        try
        {
            var result = _availableUpdate?.HasUpdate == true
                ? _availableUpdate
                : await RefreshUpdateAvailabilityAsync(showErrors: true, showProgress: true);
            if (result is null)
                return;
            if (!result.HasUpdate)
            {
                var message = string.IsNullOrWhiteSpace(result.CurrentVersion)
                    ? "No OpenKH update is currently available."
                    : $"The latest version '{result.CurrentVersion}' is already installed.";
                await MessageDialog.ShowAsync(this, "OpenKH Update", message);
                return;
            }

            var updateMessage = "A new version of OpenKH is available.\n" +
                $"Current: {result.CurrentVersion}\n" +
                $"Latest: {result.NewVersion}\n\n" +
                "Do you want to download and install it now?";
            if (!await MessageDialog.ShowAsync(this, "OpenKH Update", updateMessage, showCancel: true))
                return;

            CheckForUpdatesButton.Content = "Downloading Update...";
            await new OpenKhUpdateInstallerService(BaseDirectory).UpdateAsync(
                result.DownloadZipUrl,
                rate => Dispatcher.UIThread.Post(() => CheckForUpdatesButton.Content = $"Downloading {rate:P0}"),
                CancellationToken.None,
                Environment.ProcessPath ?? string.Empty);

            if (Avalonia.Application.Current?.ApplicationLifetime is
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        }
        catch (Exception exception)
        {
            await MessageDialog.ShowAsync(this, "OpenKH Update",
                $"OpenKH could not check for or install updates.\n\n{exception.Message}");
        }
        finally
        {
            CheckForUpdatesButton.IsEnabled = true;
            SetUpdateAvailability(_availableUpdate?.HasUpdate == true);
        }
    }

    private async Task<OpenKhReleaseUpdateCheckerService.CheckResult?> RefreshUpdateAvailabilityAsync(bool showErrors, bool showProgress)
    {
        if (showProgress)
            CheckForUpdatesButton.Content = "Checking for Updates...";
        try
        {
            var result = await new OpenKhReleaseUpdateCheckerService(BaseDirectory).CheckAsync(CancellationToken.None);
            _availableUpdate = result;
            SetUpdateAvailability(result.HasUpdate);
            return result;
        }
        catch (Exception exception)
        {
            _availableUpdate = null;
            SetUpdateAvailability(false);
            if (showErrors)
                await MessageDialog.ShowAsync(this, "OpenKH Update", $"OpenKH could not check for updates.\n\n{exception.Message}");
            return null;
        }
    }

    private void SetUpdateAvailability(bool updateAvailable)
    {
        CheckForUpdatesButton.Content = updateAvailable ? "Update Available" : "Check for Updates";
        CheckForUpdatesButton.Foreground = Brush.Parse(updateAvailable ? "#7FDCAD" : "#8FB9F8");
    }

    private async void CreateShortcut_Click(object? sender, RoutedEventArgs eventArgs)
    {
        try
        {
            var target = File.Exists(CompatibilityModManagerPath) ? CompatibilityModManagerPath : ModManagerPath;
            var path = DesktopShortcutService.CreateModManagerShortcut(target);
            await MessageDialog.ShowAsync(this, "Shortcut created",
                $"The OpenKH Mod Manager shortcut was created.\n\n{path}");
        }
        catch (Exception exception)
        {
            await MessageDialog.ShowAsync(this, "Unable to create shortcut",
                $"OpenKH could not create the shortcut.\n\n{exception.Message}");
        }
    }

    private void ShowTools_Click(object? sender, RoutedEventArgs eventArgs)
    {
        LoadTools();
        HomePanel.IsVisible = false;
        ToolsPanel.IsVisible = true;
        SearchBox.Focus();
    }

    private void ShowHome_Click(object? sender, RoutedEventArgs eventArgs) => ShowHome();

    private void ShowHome()
    {
        ToolsPanel.IsVisible = false;
        HomePanel.IsVisible = true;
        LaunchModManagerButton.Focus();
    }

    private void SearchBox_TextChanged(object? sender, TextChangedEventArgs eventArgs) => ApplyToolFilter();

    private void LaunchTool_Click(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is Button { Tag: ToolEntry tool })
            Launch(tool.ExecutablePath);
    }

    private async void ToggleFavorite_Click(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is not Button { Tag: ToolEntry tool })
            return;

        var isFavorite = _favoriteToolNames.Add(tool.Identifier);
        if (!isFavorite)
            _favoriteToolNames.Remove(tool.Identifier);
        tool.IsFavorite = isFavorite;

        try
        {
            File.WriteAllLines(FavoritesPath,
                _favoriteToolNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            tool.IsFavorite = !isFavorite;
            if (isFavorite)
                _favoriteToolNames.Remove(tool.Identifier);
            else
                _favoriteToolNames.Add(tool.Identifier);
            await MessageDialog.ShowAsync(this, "Unable to save favorites",
                $"OpenKH could not save your favorites.\n\n{exception.Message}");
        }

        ApplyToolFilter();
        ToolsList.SelectedIndex = ToolsList.ItemCount > 0 ? 0 : -1;
        if (ToolsList.SelectedItem is not null)
            ToolsList.ScrollIntoView(ToolsList.SelectedItem);
    }

    private void ToolsList_DoubleTapped(object? sender, TappedEventArgs eventArgs)
    {
        if (eventArgs.Source is Visual source && source.GetVisualAncestors().OfType<Button>().Any())
            return;
        if (ToolsList.SelectedItem is ToolEntry tool)
            Launch(tool.ExecutablePath);
    }

    private void LoadFavorites()
    {
        _favoriteToolNames.Clear();
        try
        {
            if (!File.Exists(FavoritesPath))
                return;
            foreach (var name in File.ReadLines(FavoritesPath).Select(line => line.Trim()).Where(line => line.Length > 0))
                _favoriteToolNames.Add(name);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _favoriteToolNames.Clear();
        }
    }

    private void OpenToolsFolder_Click(object? sender, RoutedEventArgs eventArgs)
    {
        if (!Directory.Exists(ApplicationsPath))
        {
            _ = MessageDialog.ShowAsync(this, "Item not found", "The Apps folder was not found.");
            return;
        }
        Launch(ApplicationsPath);
    }

    private void OpenDocumentation_Click(object? sender, RoutedEventArgs eventArgs) => Launch("https://openkh.dev/");

    private async void Launch(string target) => await TryLaunchAsync(target);

    private async Task<bool> TryLaunchAsync(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                WorkingDirectory = File.Exists(target) ? Path.GetDirectoryName(target) : AppContext.BaseDirectory,
                UseShellExecute = true,
            });
            return true;
        }
        catch (Exception exception)
        {
            await MessageDialog.ShowAsync(this, "Unable to open item", $"OpenKH could not open this item.\n\n{exception.Message}");
            return false;
        }
    }

    public void HandleControllerAction(ControllerAction action)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (action)
            {
                case ControllerAction.PreviousControl:
                case ControllerAction.PreviousItem:
                case ControllerAction.PreviousGame:
                    ControllerWindowNavigator.MoveFocus(this, -1);
                    break;
                case ControllerAction.NextControl:
                case ControllerAction.NextItem:
                case ControllerAction.NextGame:
                    ControllerWindowNavigator.MoveFocus(this, 1);
                    break;
                case ControllerAction.Confirm:
                    ActivateFocusedControl();
                    break;
                case ControllerAction.Cancel:
                    if (ToolsPanel.IsVisible)
                        ShowHome();
                    else
                        Close();
                    break;
                case ControllerAction.Secondary:
                    if (ToolsPanel.IsVisible && ToolsList.SelectedItem is ToolEntry tool)
                        ToggleFavorite(tool);
                    break;
                case ControllerAction.Install:
                    if (ToolsPanel.IsVisible && ToolsList.SelectedItem is ToolEntry selectedTool)
                        Launch(selectedTool.ExecutablePath);
                    break;
                case ControllerAction.Refresh:
                    LoadTools();
                    break;
            }
        });
    }

    private void ActivateFocusedControl()
    {
        if (FocusManager?.GetFocusedElement() is Button button && button.IsEffectivelyEnabled)
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        else if (ToolsList.SelectedItem is ToolEntry tool)
            Launch(tool.ExecutablePath);
    }

    private void ToggleFavorite(ToolEntry tool)
    {
        var button = new Button { Tag = tool };
        ToggleFavorite_Click(button, new RoutedEventArgs());
    }

    public sealed class ToolEntry
    {
        public string Identifier { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string ExecutablePath { get; }
        public bool IsFavorite { get; set; }
        public string Initial => DisplayName.Length == 0 ? "?" : DisplayName[..1].ToUpperInvariant();
        public string FavoriteSymbol => IsFavorite ? "\u2605" : "\u2606";
        public string FavoriteColor => IsFavorite ? "#FFD166" : "#93A2B8";
        public string FavoriteAction => IsFavorite
            ? $"Remove {DisplayName} from favorites"
            : $"Add {DisplayName} to favorites";

        public ToolEntry(string identifier, string displayName, string description, string executablePath, bool isFavorite)
        {
            Identifier = identifier;
            DisplayName = displayName;
            Description = description;
            ExecutablePath = executablePath;
            IsFavorite = isFavorite;
        }
    }
}
