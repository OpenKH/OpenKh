using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenKh.Tools.Launcher;
using OpenKh.Tools.Launcher.Updates;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(App))]

namespace OpenKh.Tests.Launcher.Avalonia;

public sealed class LauncherNavigationIntegrationTests
{
    private static readonly ControllerAction[] Directions =
    [
        ControllerAction.NavigateUp,
        ControllerAction.NavigateDown,
        ControllerAction.NavigateLeft,
        ControllerAction.NavigateRight
    ];

    [Fact]
    public void DataDirectoryDefaultsToInstallationDirectory()
    {
        var installationDirectory = Path.Combine(Path.GetTempPath(), "openkh-installation");

        var result = LauncherInstallation.DetectDataDirectory(installationDirectory, null);

        Assert.Equal(Path.GetFullPath(installationDirectory), result);
    }

    [Fact]
    public void DataDirectoryUsesAppImageOverride()
    {
        var installationDirectory = Path.Combine(Path.GetTempPath(), "openkh-installation");
        var dataDirectory = Path.Combine(Path.GetTempPath(), "openkh-data");

        var result = LauncherInstallation.DetectDataDirectory(installationDirectory, dataDirectory);

        Assert.Equal(Path.GetFullPath(dataDirectory), result);
    }

    [AvaloniaFact]
    public void HomeHasAConnectedControllerNavigationGraph()
    {
        using var controller = new TestControllerInputService();
        var launcher = new MainWindow(controller, checkUpdatesOnOpen: false);
        launcher.Show();
        RefreshLayout(launcher, 1100, 740);

        AssertConnected("Launcher home", launcher, launcher.HandleControllerAction);
        launcher.Close();
    }

    [AvaloniaFact]
    public void ToolsHasAConnectedControllerNavigationGraph()
    {
        using var controller = new TestControllerInputService();
        var launcher = new MainWindow(controller, checkUpdatesOnOpen: false);
        launcher.Show();

        var homePanel = launcher.FindControl<Grid>("HomePanel")!;
        var toolsPanel = launcher.FindControl<Grid>("ToolsPanel")!;
        var toolsList = launcher.FindControl<ListBox>("ToolsList")!;
        homePanel.IsVisible = false;
        toolsPanel.IsVisible = true;
        toolsList.ItemsSource = new[]
        {
            new MainWindow.ToolEntry("OpenKh.Tools.First.exe", "First tool", "First test tool", "first", false),
            new MainWindow.ToolEntry("OpenKh.Tools.Second.exe", "Second tool", "Second test tool", "second", true)
        };
        toolsList.SelectedIndex = 0;
        RefreshLayout(launcher, 1100, 740);

        Assert.Equal(2, toolsList.GetVisualDescendants().OfType<ListBoxItem>().Count());
        AssertConnected("Launcher tools", launcher, launcher.HandleControllerAction);
        launcher.Close();
    }

    [AvaloniaFact]
    public void MessageDialogHasAConnectedControllerNavigationGraph()
    {
        var dialogType = typeof(MainWindow).Assembly.GetType("OpenKh.Tools.Launcher.MessageDialog", throwOnError: true)!;
        var dialog = (Window)Activator.CreateInstance(dialogType, "Test", "Test message", true)!;
        var handler = dialogType.GetMethod(
            "HandleControllerAction",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        dialog.Show();
        RefreshLayout(dialog, 520, 260);

        AssertConnected(
            "Launcher message dialog",
            dialog,
            action => handler.Invoke(dialog, [action]));
        dialog.Close();
    }

    private static void AssertConnected(string name, Control root, Action<ControllerAction> handle)
    {
        var targets = GetNavigationTargets(root);
        Assert.True(targets.Length > 0, $"{name} has no controller navigation targets.");

        var edges = targets.ToDictionary(target => target, _ => new HashSet<Control>());
        foreach (var source in targets)
        {
            Assert.True(FocusTarget(source), $"{name} could not focus {Describe(source)}.");
            foreach (var direction in Directions)
            {
                Assert.True(FocusTarget(source), $"{name} lost {Describe(source)} before {direction}.");
                handle(direction);
                Dispatcher.UIThread.RunJobs();
                var focused = TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control;
                var target = GetOutermostTarget(focused);
                Assert.True(target is not null && targets.Contains(target),
                    $"{name} moved outside its active screen after {direction} from {Describe(source)}.");
                edges[source].Add(target!);
            }
        }

        var visited = new HashSet<Control> { targets[0] };
        var pending = new Queue<Control>();
        pending.Enqueue(targets[0]);
        while (pending.TryDequeue(out var current))
        {
            foreach (var next in edges[current])
            {
                if (visited.Add(next))
                    pending.Enqueue(next);
            }
        }

        var unreachable = targets.Where(target => !visited.Contains(target)).Select(Describe).ToArray();
        Assert.True(unreachable.Length == 0,
            $"{name} has unreachable controls: {string.Join(", ", unreachable)}.");
    }

    private static Control[] GetNavigationTargets(Control root) => root.GetVisualDescendants()
        .OfType<Control>()
        .Where(IsAvailable)
        .Where(IsNavigationKind)
        .Where(control => !control.GetVisualAncestors()
            .OfType<Control>()
            .Any(ancestor => IsAvailable(ancestor) && IsNavigationKind(ancestor)))
        .ToArray();

    private static Control? GetOutermostTarget(Control? control) => control?
        .GetVisualAncestors()
        .Prepend(control)
        .OfType<Control>()
        .Where(IsAvailable)
        .Where(IsNavigationKind)
        .LastOrDefault();

    private static bool IsAvailable(Control control) =>
        control.Focusable &&
        control.IsVisible &&
        control.IsEffectivelyEnabled &&
        control.GetVisualAncestors()
            .OfType<Control>()
            .All(ancestor => ancestor.IsVisible && ancestor.IsEffectivelyEnabled);

    private static bool IsNavigationKind(Control control) =>
        control is Button or TextBox or ComboBox or CheckBox or ToggleSwitch or Expander or ListBoxItem;

    private static bool FocusTarget(Control target)
    {
        if (target.Focus(NavigationMethod.Directional))
            return true;

        return target.GetVisualDescendants()
            .OfType<Control>()
            .Where(IsAvailable)
            .Any(control => control.Focus(NavigationMethod.Directional));
    }

    private static string Describe(Control control) => control switch
    {
        Button button => $"Button:{button.Name ?? button.Content?.ToString()}",
        TextBox textBox => $"TextBox:{textBox.Name ?? textBox.PlaceholderText}",
        ComboBox comboBox => $"ComboBox:{comboBox.Name}",
        ListBoxItem item => $"ListBoxItem:{item.DataContext}",
        _ => $"{control.GetType().Name}:{control.Name}"
    };

    private static void RefreshLayout(Window window, double width, double height)
    {
        window.Width = width;
        window.Height = height;
        Dispatcher.UIThread.RunJobs();
        window.InvalidateMeasure();
        (window.Content as Control)?.InvalidateMeasure();
        window.Measure(new Size(width, height));
        window.Arrange(new Rect(0, 0, width, height));
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class TestControllerInputService : IControllerInputService
    {
        public event Action<ControllerAction>? ActionTriggered;
        public event Action? ConnectionChanged { add { } remove { } }
        public event Action? StatusChanged { add { } remove { } }
        public bool IsConnected => true;
        public string StatusText => "Controller connected";
        public string NavigationHelpText => "Controller navigation";
        public void Start() { }
        public void Dispatch(ControllerAction action) => ActionTriggered?.Invoke(action);
        public IDisposable Capture(Action<ControllerAction> handler) => new EmptyDisposable();
        public void Dispose() { }

        private sealed class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
