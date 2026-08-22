using global::Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using ModViews = OpenKh.Tools.ModsManager.Avalonia.Views;
using Xunit;

namespace OpenKh.Tests.ModsManager.Avalonia;

public sealed class AllWindowNavigationIntegrationTests
{
    private static readonly ControllerAction[] Directions =
    [
        ControllerAction.NavigateUp,
        ControllerAction.NavigateDown,
        ControllerAction.NavigateLeft,
        ControllerAction.NavigateRight
    ];

    [AvaloniaFact]
    public void EveryModManagerDialogHasAConnectedControllerNavigationGraph()
    {
        var screens = new (string Name, Func<ModViews.EmbeddedDialogControl> Create, Action<ModViews.EmbeddedDialogControl, ControllerAction> Handle)[]
        {
            ("Collection settings", () => new ModViews.CollectionSettingsWindow(), (view, action) => ((ModViews.CollectionSettingsWindow)view).HandleControllerAction(action)),
            ("Confirmation", () => new ModViews.ConfirmationWindow(), (view, action) => ((ModViews.ConfirmationWindow)view).HandleControllerAction(action)),
            ("Controller keyboard", () => new ModViews.ControllerKeyboardWindow(), (view, action) => ((ModViews.ControllerKeyboardWindow)view).HandleControllerAction(action)),
            ("Creator", () => new ModViews.CreatorWindow(), (view, action) => ((ModViews.CreatorWindow)view).HandleControllerAction(action)),
            ("Info", () => new ModViews.InfoWindow(), (view, action) => ((ModViews.InfoWindow)view).HandleControllerAction(action)),
            ("Install mods", () => new ModViews.InstallModWindow(), (view, action) => ((ModViews.InstallModWindow)view).HandleControllerAction(action)),
            ("Browse mods", () => new ModViews.OnlineModsWindow(), (view, action) => ((ModViews.OnlineModsWindow)view).HandleControllerAction(action)),
            ("Presets", () => new ModViews.PresetsWindow(), (view, action) => ((ModViews.PresetsWindow)view).HandleControllerAction(action)),
            ("Settings", () => new ModViews.SettingsWindow(), (view, action) => ((ModViews.SettingsWindow)view).HandleControllerAction(action)),
            ("Setup", () => new ModViews.SetupWindow(), (view, action) => ((ModViews.SetupWindow)view).HandleControllerAction(action)),
            ("Target files", () => new ModViews.TargetFilesWindow(), (view, action) => ((ModViews.TargetFilesWindow)view).HandleControllerAction(action))
        };

        var windowSizes = new[]
        {
            new Size(1500, 1000),
            new Size(960, 640)
        };

        foreach (var windowSize in windowSizes)
        {
            foreach (var screen in screens)
            {
                var view = screen.Create();
                var window = ShowContent(view, windowSize.Width, windowSize.Height);
                AssertConnected(
                    $"{screen.Name} at {windowSize.Width}x{windowSize.Height}",
                    view,
                    action => screen.Handle(view, action));
                window.Close();
            }
        }
    }

    private static void AssertConnected(string name, Control root, Action<ControllerAction> handle)
        => AssertConnectedCore(name, root, direction => handle(direction));

    private static void AssertConnectedCore(string name, Control root, Action<ControllerAction> handle)
    {
        var targets = GetNavigationTargets(root);
        Assert.True(targets.Length > 0, $"{name} has no controller navigation targets.");

        var edges = targets.ToDictionary(target => target, _ => new HashSet<Control>());
        foreach (var source in targets)
        {
            source.BringIntoView();
            Dispatcher.UIThread.RunJobs();
            Assert.True(FocusTarget(source), $"{name} could not focus {Describe(source)}.");
            foreach (var direction in Directions)
            {
                source.BringIntoView();
                Dispatcher.UIThread.RunJobs();
                Assert.True(FocusTarget(source), $"{name} lost {Describe(source)} before {direction}.");
                handle(direction);
                Dispatcher.UIThread.RunJobs();
                var focused = TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control;
                var target = GetOutermostTarget(focused);
                Assert.True(target is not null && targets.Contains(target), $"{name} moved outside its active screen after {direction} from {Describe(source)}.");
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
        var graph = string.Join("; ", edges.Select(edge =>
            $"{Describe(edge.Key)} -> {string.Join(" | ", edge.Value.Select(Describe))}"));
        var bounds = string.Join("; ", targets.Select(target =>
        {
            var transform = target.TransformToVisual(root);
            var position = transform is null
                ? target.Bounds
                : new Rect(target.Bounds.Size).TransformToAABB(transform.Value);
            return $"{Describe(target)}={position}";
        }));
        Assert.True(unreachable.Length == 0,
            $"{name} has unreachable controls: {string.Join(", ", unreachable)}. Graph: {graph}. Bounds: {bounds}");
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

    private static string Describe(Control control) => control switch
    {
        CheckBox checkBox => $"CheckBox:{checkBox.Name ?? checkBox.Content?.ToString()}",
        Button button => $"Button:{button.Name ?? button.Content?.ToString()}",
        TextBox textBox => $"TextBox:{textBox.Name ?? textBox.PlaceholderText}",
        ComboBox comboBox => $"ComboBox:{comboBox.Name}",
        ListBoxItem item => $"ListBoxItem:{item.DataContext}",
        _ => $"{control.GetType().Name}:{control.Name}"
    };

    private static Window ShowContent(Control content, double width, double height)
    {
        if (Application.Current?.Styles.OfType<FluentTheme>().Any() == false)
            Application.Current.Styles.Add(new FluentTheme());

        var window = content as Window ?? new Window { Content = content };
        window.Width = width;
        window.Height = height;
        window.Show();
        RefreshLayout(window, width, height);
        return window;
    }

    private static void RefreshLayout(Window window, double width, double height)
    {
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

    private static bool FocusTarget(Control target)
    {
        if (target.Focus(NavigationMethod.Directional))
            return true;

        return target.GetVisualDescendants()
            .OfType<Control>()
            .Where(IsAvailable)
            .Any(control => control.Focus(NavigationMethod.Directional));
    }

}
