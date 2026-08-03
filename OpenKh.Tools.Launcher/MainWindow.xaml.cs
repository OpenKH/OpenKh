using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Launcher;

public partial class MainWindow : Window
{
    private const string ModManagerExecutable = "OpenKh.Tools.ModsManager.exe";
    private const string ApplicationsDirectory = "Apps";
    private const string ModManagerDirectory = "ModManager";
    private const string AdvancedToolsDirectory = "AdvancedTools";

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
    private string BaseDirectory => AppContext.BaseDirectory;
    private string ModManagerPath
    {
        get
        {
            var packagedPath = Path.Combine(
                BaseDirectory,
                ApplicationsDirectory,
                ModManagerDirectory,
                ModManagerExecutable
            );

            return File.Exists(packagedPath)
                ? packagedPath
                : Path.Combine(BaseDirectory, ModManagerExecutable);
        }
    }
    private string AdvancedToolsPath => Path.Combine(BaseDirectory, AdvancedToolsDirectory);

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
        ModManagerStatusText.Text = modManagerAvailable ? "Ready" : "Mod Manager was not found";
        ModManagerStatusText.Foreground = modManagerAvailable
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(127, 220, 173))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(238, 137, 137));

        LoadTools();
    }

    private void LoadTools()
    {
        _allTools.Clear();

        if (Directory.Exists(AdvancedToolsPath))
        {
            _allTools.AddRange(
                Directory.EnumerateFiles(AdvancedToolsPath, "OpenKh.Tools.*.exe", SearchOption.TopDirectoryOnly)
                    .Where(path => !Path.GetFileName(path).Equals(ModManagerExecutable, StringComparison.OrdinalIgnoreCase))
                    .Select(CreateToolEntry)
                    .OrderBy(tool => tool.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            );
        }

        ToolCountText.Text = _allTools.Count == 0
            ? "Tools are installed with the full OpenKH package"
            : $"{_allTools.Count} tools available";

        ApplyToolFilter();
    }

    private static ToolEntry CreateToolEntry(string executablePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(executablePath);
        var shortName = fileNameWithoutExtension.StartsWith("OpenKh.Tools.", StringComparison.OrdinalIgnoreCase)
            ? fileNameWithoutExtension["OpenKh.Tools.".Length..]
            : fileNameWithoutExtension;
        var displayName = HumanizeName(shortName);
        var description = ToolDescriptions.TryGetValue(shortName, out var knownDescription)
            ? knownDescription
            : "Open a specialized OpenKH modding utility.";

        return new ToolEntry(displayName, description, executablePath);
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
        var filteredTools = string.IsNullOrWhiteSpace(query)
            ? _allTools
            : _allTools
                .Where(tool => tool.DisplayName.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                    || tool.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

        if (ToolsList != null)
            ToolsList.ItemsSource = filteredTools;

        if (ToolsStatusText != null)
        {
            ToolsStatusText.Text = !Directory.Exists(AdvancedToolsPath)
                ? "The AdvancedTools folder is not available in this installation."
                : $"Showing {filteredTools.Count} of {_allTools.Count} tools";
        }
    }

    private void LaunchModManager_Click(object sender, RoutedEventArgs e) => Launch(ModManagerPath);

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

    private void ToolsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ToolsList.SelectedItem is ToolEntry tool)
            Launch(tool.ExecutablePath);
    }

    private void OpenToolsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(AdvancedToolsPath))
        {
            ShowMissingItem("The AdvancedTools folder was not found.");
            return;
        }

        Launch(AdvancedToolsPath);
    }

    private void OpenDocumentation_Click(object sender, RoutedEventArgs e) => Launch("https://openkh.dev/");

    private static void Launch(string target)
    {
        try
        {
            var workingDirectory = File.Exists(target)
                ? Path.GetDirectoryName(target)
                : AppContext.BaseDirectory;

            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                WorkingDirectory = workingDirectory,
                UseShellExecute = true,
            });
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

    private sealed record ToolEntry(string DisplayName, string Description, string ExecutablePath)
    {
        public string Initial => DisplayName.Length == 0 ? "?" : DisplayName[..1].ToUpperInvariant();
    }
}
