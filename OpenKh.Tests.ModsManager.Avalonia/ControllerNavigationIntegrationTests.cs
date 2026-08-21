using global::Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Avalonia.ViewModels;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using Xunit;

namespace OpenKh.Tests.ModsManager.Avalonia;

public class ControllerNavigationIntegrationTests
{
    [AvaloniaFact]
    public void BrowseGridMovesDownToTheRenderedCardBelow()
    {
        var cards = Enumerable.Range(0, 6)
            .Select(index => new Button { Content = $"Mod {index}", Width = 180, Height = 220 })
            .ToArray();
        var window = ShowCanvas(640, 500, cards.Select((card, index) =>
            (Control: (Control)card, X: (index % 3) * 200d, Y: (index / 3) * 240d)));

        Assert.True(cards[1].Focus());
        ControllerWindowNavigator.MoveFocus(window, NavigationDirection.Down);

        Assert.Same(cards[4], window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void BrowseGridMovesRightIntoTheDetailsAction()
    {
        var cards = Enumerable.Range(0, 3)
            .Select(index => new Button { Content = $"Mod {index}", Width = 180, Height = 220 })
            .ToArray();
        var installButton = new Button { Content = "Install selected mod", Width = 240, Height = 60 };
        var window = ShowCanvas(1000, 500,
        [
            (cards[0], 0, 0),
            (cards[1], 200, 0),
            (cards[2], 400, 0),
            (installButton, 700, 80)
        ]);

        Assert.True(cards[2].Focus());
        ControllerWindowNavigator.MoveFocus(window, NavigationDirection.Right);

        Assert.Same(installButton, window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void MainMenuCanMoveRightIntoTheInstalledModList()
    {
        var menuButton = new Button { Content = "Setup", Width = 220, Height = 60 };
        var list = new ListBox
        {
            Width = 600,
            Height = 180,
            ItemsSource = new[] { "Installed mod" }
        };
        var window = ShowCanvas(1000, 600,
        [
            (menuButton, 0, 300),
            (list, 300, 240)
        ]);
        var item = list.GetVisualDescendants().OfType<ListBoxItem>().Single();

        Assert.True(menuButton.Focus());
        ControllerWindowNavigator.MoveFocus(window, NavigationDirection.Right);

        Assert.Same(item, window.FocusManager?.GetFocusedElement());
        Assert.Equal("Installed mod", list.SelectedItem);
        window.Close();
    }

    [AvaloniaFact]
    public void SetupMovesRightToBrowseAndDownToTheNextField()
    {
        var folderInput = new TextBox { Width = 800, Height = 50 };
        var browseButton = new Button { Content = "Browse", Width = 120, Height = 50 };
        var platformCombo = new ComboBox
        {
            Width = 220,
            Height = 50,
            ItemsSource = new[] { "Epic Games Store", "Steam", "Other" },
            SelectedIndex = 1
        };
        var window = ShowCanvas(1000, 400,
        [
            (folderInput, 0, 100),
            (browseButton, 820, 100),
            (platformCombo, 0, 180)
        ]);

        Assert.True(folderInput.Focus());
        ControllerWindowNavigator.MoveFocus(window, NavigationDirection.Right);
        Assert.Same(browseButton, window.FocusManager?.GetFocusedElement());

        Assert.True(folderInput.Focus());
        ControllerWindowNavigator.MoveFocus(window, NavigationDirection.Down);
        Assert.Same(platformCombo, window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void FocusedListItemBecomesTheSelectedItem()
    {
        var list = new ListBox
        {
            Width = 500,
            Height = 240,
            ItemsSource = new[] { "First mod", "Second mod", "Third mod" }
        };
        var window = ShowCanvas(600, 300, [(list, 20, 20)]);
        var items = list.GetVisualDescendants().OfType<ListBoxItem>().ToArray();

        Assert.True(items[2].Focus());
        ControllerWindowNavigator.SyncSelectionWithFocus(window);

        Assert.Equal("Third mod", list.SelectedItem);
        window.Close();
    }

    [AvaloniaFact]
    public void DialogUsesItsSelectedItemWhenFocusIsBehindIt()
    {
        var backgroundButton = new Button { Content = "Background", Width = 220, Height = 60 };
        var list = new ListBox
        {
            Width = 500,
            Height = 260,
            ItemsSource = new[] { "First mod", "Second mod", "Third mod" },
            SelectedIndex = 1
        };
        var dialogRoot = new Grid { Width = 600, Height = 300 };
        dialogRoot.Children.Add(list);
        var window = ShowCanvas(1000, 600,
        [
            (backgroundButton, 0, 0),
            (dialogRoot, 300, 150)
        ]);
        var items = list.GetVisualDescendants().OfType<ListBoxItem>().ToArray();

        Assert.True(backgroundButton.Focus());
        ControllerWindowNavigator.MoveFocus(dialogRoot, NavigationDirection.Down);

        Assert.Equal("Third mod", list.SelectedItem);
        Assert.Same(items[2], window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void SetupIgnoresFocusBehindTheDialog()
    {
        var backgroundButton = new Button { Content = "Background", Width = 220, Height = 60 };
        var folderInput = new TextBox { Width = 500, Height = 50 };
        var browseButton = new Button { Content = "Browse", Width = 120, Height = 50 };
        var dialogRoot = new Canvas { Width = 700, Height = 300 };
        Canvas.SetLeft(folderInput, 0);
        Canvas.SetTop(folderInput, 100);
        Canvas.SetLeft(browseButton, 520);
        Canvas.SetTop(browseButton, 100);
        dialogRoot.Children.Add(folderInput);
        dialogRoot.Children.Add(browseButton);
        var window = ShowCanvas(1000, 600,
        [
            (backgroundButton, 0, 0),
            (dialogRoot, 260, 120)
        ]);

        Assert.True(backgroundButton.Focus());
        ControllerWindowNavigator.MoveFocus(dialogRoot, NavigationDirection.Right);

        Assert.Same(browseButton, window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void DisabledControlsAreSkipped()
    {
        var source = new Button { Content = "Source", Width = 140, Height = 50 };
        var disabled = new Button { Content = "Disabled", Width = 140, Height = 50, IsEnabled = false };
        var available = new Button { Content = "Available", Width = 140, Height = 50 };
        var window = ShowCanvas(600, 200,
        [
            (source, 0, 50),
            (disabled, 170, 50),
            (available, 340, 50)
        ]);

        Assert.True(source.Focus());
        ControllerWindowNavigator.MoveFocus(window, NavigationDirection.Right);

        Assert.Same(available, window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void NavigationDoesNotWrapWhenThereIsNoControlInThatDirection()
    {
        var left = new Button { Content = "Left", Width = 140, Height = 50 };
        var right = new Button { Content = "Right", Width = 140, Height = 50 };
        var window = ShowCanvas(400, 200,
        [
            (left, 0, 50),
            (right, 180, 50)
        ]);

        Assert.True(right.Focus());
        ControllerWindowNavigator.MoveFocus(window, NavigationDirection.Right);

        Assert.Same(right, window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void ActualBrowseModsMovesToTheCardBelowAndKeepsSelectionInSync()
    {
        var browse = new OnlineModsWindow();
        var list = browse.FindControl<ListBox>("ModsList")!;
        var mods = Enumerable.Range(0, 9)
            .Select(index => new BrowseModStub($"Mod {index}"))
            .ToArray();
        list.ItemsSource = mods;
        list.SelectedIndex = 1;
        var window = ShowContent(browse, 1400, 900);
        var items = list.GetVisualDescendants().OfType<ListBoxItem>().ToArray();
        var source = items[1];
        var sourceCenter = GetCenter(source, browse);
        var expected = items
            .Where(item => GetCenter(item, browse).Y > sourceCenter.Y + 1)
            .OrderBy(item => Math.Abs(GetCenter(item, browse).X - sourceCenter.X))
            .ThenBy(item => GetCenter(item, browse).Y)
            .First();

        Assert.True(source.Focus());
        ControllerWindowNavigator.MoveFocus(browse, NavigationDirection.Down);

        Assert.Same(expected, window.FocusManager?.GetFocusedElement());
        Assert.Same(expected.DataContext, list.SelectedItem);
        window.Close();
    }

    [AvaloniaFact]
    public void ActualMainWindowCanMoveFromSetupIntoTheInstalledModList()
    {
        var main = new MainWindow();
        var list = main.FindControl<ListBox>("ModList")!;
        var window = ShowContent(main, 1280, 800);
        var installedItem = new ListBoxItem
        {
            Content = "Installed mod",
            Width = 520,
            Height = 72,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top
        };
        Grid.SetRow(installedItem, 1);
        ((Grid)list.Parent!).Children.Add(installedItem);
        RefreshLayout(window, 1280, 800);
        var setupButton = main.FindControl<Button>("SetupNavigationButton")!;
        setupButton.Command = new RelayCommand(() => { });

        Assert.True(
            setupButton.Focus(),
            $"Setup focus failed. Visible: {setupButton.IsVisible}; enabled: {setupButton.IsEffectivelyEnabled}; focusable: {setupButton.Focusable}; bounds: {setupButton.Bounds}.");
        ControllerWindowNavigator.MoveFocus(main, NavigationDirection.Right);

        Assert.Same(installedItem, window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    [AvaloniaFact]
    public void ActualSetupCanNavigateFromLuaBackendToGameDataAndBack()
    {
        var setup = new SetupWindow();
        var luaKh3D = setup.FindControl<CheckBox>("LuaKh3DCheckBox")!;
        var gameData = setup.FindControl<TextBox>("GameDataTextBox")!;
        var window = ShowContent(setup, 1600, 1000);

        Assert.True(luaKh3D.Focus());
        var downPath = MoveAndTrace(setup, NavigationDirection.Down, 30);
        Assert.True(downPath.Contains(gameData), string.Join(" > ", downPath.Select(Describe)));
        var upPath = MoveAndTrace(setup, NavigationDirection.Up, 30);
        Assert.True(upPath.Contains(luaKh3D), string.Join(" > ", upPath.Select(Describe)));
        window.Close();
    }

    private static IReadOnlyList<Control?> MoveAndTrace(
        Control root,
        NavigationDirection direction,
        int maximumMoves)
    {
        var path = new List<Control?>();
        for (var move = 0; move < maximumMoves; move++)
        {
            path.Add(TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control);
            ControllerWindowNavigator.MoveFocus(root, direction);
        }

        path.Add(TopLevel.GetTopLevel(root)?.FocusManager?.GetFocusedElement() as Control);
        return path;
    }

    private static string Describe(Control? control) => control switch
    {
        null => "null",
        CheckBox checkBox => $"CheckBox:{checkBox.Name ?? checkBox.Content?.ToString()}",
        Button button => $"Button:{button.Name ?? button.Content?.ToString()}",
        TextBox textBox => $"TextBox:{textBox.Name}",
        ComboBox comboBox => $"ComboBox:{comboBox.Name}",
        ListBoxItem item => $"ListBoxItem:{item.DataContext}",
        _ => $"{control.GetType().Name}:{control.Name}"
    };

    private static global::Avalonia.Point GetCenter(Control control, Control root)
    {
        var transform = control.TransformToVisual(root)!.Value;
        var bounds = new global::Avalonia.Rect(control.Bounds.Size).TransformToAABB(transform);
        return new global::Avalonia.Point(bounds.Center.X, bounds.Center.Y);
    }

    private static Window ShowCanvas(
        double width,
        double height,
        IEnumerable<(Control Control, double X, double Y)> controls)
    {
        if (global::Avalonia.Application.Current?.Styles.OfType<FluentTheme>().Any() == false)
            global::Avalonia.Application.Current.Styles.Add(new FluentTheme());

        var canvas = new Canvas();
        foreach (var (control, x, y) in controls)
        {
            Canvas.SetLeft(control, x);
            Canvas.SetTop(control, y);
            canvas.Children.Add(control);
        }

        var window = new Window
        {
            Width = width,
            Height = height,
            Content = canvas
        };
        window.Show();
        window.Measure(new global::Avalonia.Size(width, height));
        window.Arrange(new global::Avalonia.Rect(0, 0, width, height));
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Window ShowContent(Control content, double width, double height)
    {
        if (global::Avalonia.Application.Current?.Styles.OfType<FluentTheme>().Any() == false)
            global::Avalonia.Application.Current.Styles.Add(new FluentTheme());

        var window = content as Window ?? new Window { Content = content };
        window.Width = width;
        window.Height = height;
        window.Show();
        RefreshLayout(window, width, height);
        return window;
    }

    private static void RefreshLayout(Window window, double width, double height)
    {
        window.Measure(new global::Avalonia.Size(width, height));
        window.Arrange(new global::Avalonia.Rect(0, 0, width, height));
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed record BrowseModStub(string Title)
    {
        public string Author => "Author";
        public string Initial => Title[..1];
        public object? PreviewImage => null;
        public object? IconImage => null;
    }
}
