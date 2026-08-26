using OpenKh.Tools.ModsManager.Services;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenKh.Tools.Launcher;

public partial class MainWindow : Window
{
    private const string ModManagerExecutable = "OpenKh.Tools.ModsManager.exe";
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

    private readonly List<ToolEntry> _allTools = new();
    private readonly HashSet<string> _favoriteToolNames = new(StringComparer.OrdinalIgnoreCase);
    private OpenkhUpdateCheckerService.CheckResult? _availableUpdate;
    private string BaseDirectory => OpenkhInstallation.Directory;
    private string ModManagerPath => OpenkhInstallation.GetModManagerExecutable(BaseDirectory);
    private string ApplicationsPath => Path.Combine(BaseDirectory, ApplicationsDirectory);
    private string FavoritesPath => Path.Combine(BaseDirectory, FavoritesFileName);
    private string CompatibilityModManagerPath => Path.Combine(BaseDirectory, ModManagerExecutable);

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var version = FileVersionInfo.GetVersionInfo(Environment.ProcessPath!).ProductVersion;
        VersionText.Text = string.IsNullOrWhiteSpace(version) ? string.Empty : $"Version {version}";

        var modManagerAvailable = File.Exists(ModManagerPath);
        LaunchModManagerButton.IsEnabled = modManagerAvailable;
        CheckForUpdatesButton.IsEnabled = true;
        CreateShortcutButton.IsEnabled = File.Exists(CompatibilityModManagerPath);
        ModManagerStatusText.Text = modManagerAvailable ? string.Empty : "Mod Manager was not found";
        ModManagerStatusText.Visibility = modManagerAvailable ? Visibility.Collapsed : Visibility.Visible;

        LoadFavorites();
        LoadTools();
        _ = RefreshUpdateAvailabilityAsync(showErrors: false, showProgress: false);
    }

    private void LoadTools()
    {
        _allTools.Clear();

        if (Directory.Exists(ApplicationsPath))
        {
            _allTools.AddRange(
                Directory.EnumerateFiles(ApplicationsPath, "OpenKh.Tools.*.exe", SearchOption.TopDirectoryOnly)
                    .Where(path => !Path.GetFileName(path).Equals(ModManagerExecutable, StringComparison.OrdinalIgnoreCase))
                    .Select(CreateToolEntry)
            );
        }

        ToolCountText.Text = _allTools.Count == 0
            ? "Tools are installed with the full OpenKH package"
            : $"{_allTools.Count} tools available";

        ApplyToolFilter();
    }

    private ToolEntry CreateToolEntry(string executablePath)
    {
        var identifier = Path.GetFileName(executablePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(executablePath);
        var shortName = fileNameWithoutExtension.StartsWith("OpenKh.Tools.", StringComparison.OrdinalIgnoreCase)
            ? fileNameWithoutExtension["OpenKh.Tools.".Length..]
            : fileNameWithoutExtension;
        var displayName = HumanizeName(shortName);
        var description = ToolDescriptions.TryGetValue(shortName, out var knownDescription)
            ? knownDescription
            : "Open a specialized OpenKH modding utility.";

        return new ToolEntry(
            identifier,
            displayName,
            description,
            executablePath,
            _favoriteToolNames.Contains(identifier)
        );
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
            : _allTools
                .Where(tool => tool.DisplayName.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                    || tool.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase));
        var filteredTools = matchingTools
            .OrderByDescending(tool => tool.IsFavorite)
            .ThenBy(tool => tool.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (ToolsList != null)
            ToolsList.ItemsSource = filteredTools;

        if (ToolsStatusText != null)
        {
            ToolsStatusText.Text = !Directory.Exists(ApplicationsPath)
                ? "The Apps folder is not available in this installation."
                : $"Showing {filteredTools.Count} of {_allTools.Count} tools";
        }
    }

    private void LaunchModManager_Click(object sender, RoutedEventArgs e) => Launch(ModManagerPath);

    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        CheckForUpdatesButton.IsEnabled = false;

        try
        {
            var checkResult = _availableUpdate?.HasUpdate == true
                ? _availableUpdate
                : await RefreshUpdateAvailabilityAsync(showErrors: true, showProgress: true);
            if (checkResult == null)
                return;

            if (!checkResult.HasUpdate)
            {
                var message = string.IsNullOrWhiteSpace(checkResult.CurrentVersion)
                    ? "No OpenKH update is currently available."
                    : $"The latest version '{checkResult.CurrentVersion}' is already installed.";
                MessageBox.Show(this, message, "OpenKH Update", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var updateMessage = "A new version of OpenKH is available.\n" +
                $"Current: {checkResult.CurrentVersion}\n" +
                $"Latest: {checkResult.NewVersion}\n\n" +
                "Do you want to download and install it now?";
            if (MessageBox.Show(
                this,
                updateMessage,
                "OpenKH Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) != MessageBoxResult.Yes)
            {
                return;
            }

            CheckForUpdatesButton.Content = "Downloading Update...";
            var launcherPath = Path.Combine(OpenkhInstallation.Directory, "OpenKh.Launcher.exe");
            await new OpenkhUpdateProceederService().UpdateAsync(
                checkResult.DownloadZipUrl,
                rate => Dispatcher.Invoke(() =>
                    CheckForUpdatesButton.Content = $"Downloading {rate:P0}"),
                CancellationToken.None,
                launcherPath
            );

            Application.Current.Shutdown();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                $"OpenKH could not check for or install updates.\n\n{exception.Message}",
                "OpenKH Update",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        finally
        {
            CheckForUpdatesButton.IsEnabled = true;
            SetUpdateAvailability(_availableUpdate?.HasUpdate == true);
        }
    }

    private async Task<OpenkhUpdateCheckerService.CheckResult?> RefreshUpdateAvailabilityAsync(
        bool showErrors,
        bool showProgress
    )
    {
        if (showProgress)
            CheckForUpdatesButton.Content = "Checking for Updates...";

        try
        {
            var checkResult = await new OpenkhUpdateCheckerService().CheckAsync(CancellationToken.None);
            _availableUpdate = checkResult;
            SetUpdateAvailability(checkResult.HasUpdate);
            return checkResult;
        }
        catch (Exception exception)
        {
            _availableUpdate = null;
            SetUpdateAvailability(false);

            if (showErrors)
            {
                MessageBox.Show(
                    this,
                    $"OpenKH could not check for updates.\n\n{exception.Message}",
                    "OpenKH Update",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return null;
        }
    }

    private void SetUpdateAvailability(bool updateAvailable)
    {
        CheckForUpdatesButton.Content = updateAvailable ? "Update Available" : "Check for Updates";
        CheckForUpdatesButton.Foreground = new SolidColorBrush(updateAvailable
            ? Color.FromRgb(127, 220, 173)
            : Color.FromRgb(143, 185, 248));
    }

    private void CreateShortcut_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var shortcutPath = DesktopShortcutService.CreateModManagerShortcut(CompatibilityModManagerPath);
            MessageBox.Show(
                $"The OpenKH Mod Manager shortcut was created on your desktop.\n\n{shortcutPath}",
                "Shortcut created",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"OpenKH could not create the desktop shortcut.\n\n{exception.Message}",
                "Unable to create shortcut",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ShowTools_Click(object sender, RoutedEventArgs e)
    {
        LoadTools();
        HomePanel.Visibility = Visibility.Collapsed;
        ToolsPanel.Visibility = Visibility.Visible;
        SearchBox.Focus();
    }

    private void ShowHome_Click(object sender, RoutedEventArgs e)
    {
        ToolsPanel.Visibility = Visibility.Collapsed;
        HomePanel.Visibility = Visibility.Visible;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyToolFilter();

    private void LaunchTool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ToolEntry tool })
            Launch(tool.ExecutablePath);
    }

    private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ToolEntry tool })
            return;

        var isFavorite = _favoriteToolNames.Add(tool.Identifier);
        if (!isFavorite)
            _favoriteToolNames.Remove(tool.Identifier);

        tool.IsFavorite = isFavorite;

        try
        {
            File.WriteAllLines(
                FavoritesPath,
                _favoriteToolNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            );
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            tool.IsFavorite = !isFavorite;
            if (isFavorite)
                _favoriteToolNames.Remove(tool.Identifier);
            else
                _favoriteToolNames.Add(tool.Identifier);

            MessageBox.Show(
                this,
                $"OpenKH could not save your favorites.\n\n{exception.Message}",
                "Unable to save favorites",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        ApplyToolFilter();
        if (ToolsList.Items.Count > 0)
        {
            ToolsList.SelectedItem = null;
            ToolsList.ScrollIntoView(ToolsList.Items[0]);
        }
    }

    private void ToolsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source
            && FindVisualAncestor<Button>(source) != null)
        {
            return;
        }

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

            foreach (var favoriteToolName in File.ReadLines(FavoritesPath)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0))
            {
                _favoriteToolNames.Add(favoriteToolName);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _favoriteToolNames.Clear();
        }
    }

    private static T? FindVisualAncestor<T>(DependencyObject source) where T : DependencyObject
    {
        DependencyObject? current = source;
        while (current != null)
        {
            if (current is T ancestor)
                return ancestor;

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void OpenToolsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(ApplicationsPath))
        {
            ShowMissingItem("The Apps folder was not found.");
            return;
        }

        Launch(ApplicationsPath);
    }

    private void OpenDocumentation_Click(object sender, RoutedEventArgs e) => Launch("https://openkh.dev/");

    private static void Launch(string target, params string[] arguments)
    {
        try
        {
            var workingDirectory = File.Exists(target)
                ? Path.GetDirectoryName(target)
                : AppContext.BaseDirectory;

            var startInfo = new ProcessStartInfo
            {
                FileName = target,
                WorkingDirectory = workingDirectory,
                UseShellExecute = true,
            };

            foreach (var argument in arguments)
                startInfo.ArgumentList.Add(argument);

            Process.Start(startInfo);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"OpenKH could not open this item.\n\n{exception.Message}",
                "Unable to open item",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private static void ShowMissingItem(string message)
    {
        MessageBox.Show(message, "Item not found", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private sealed class ToolEntry
    {
        public string Identifier { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string ExecutablePath { get; }
        public bool IsFavorite { get; set; }
        public string Initial => DisplayName.Length == 0 ? "?" : DisplayName[..1].ToUpperInvariant();
        public string FavoriteSymbol => IsFavorite ? "★" : "☆";
        public string FavoriteAction => IsFavorite
            ? $"Remove {DisplayName} from favorites"
            : $"Add {DisplayName} to favorites";

        public ToolEntry(
            string identifier,
            string displayName,
            string description,
            string executablePath,
            bool isFavorite
        )
        {
            Identifier = identifier;
            DisplayName = displayName;
            Description = description;
            ExecutablePath = executablePath;
            IsFavorite = isFavorite;
        }
    }
}
