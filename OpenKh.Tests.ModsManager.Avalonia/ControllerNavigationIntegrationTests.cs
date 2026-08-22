using System.Collections.ObjectModel;
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
using OpenKh.Tools.ModsManager.Core;
using System.Reflection;
using Xunit;

namespace OpenKh.Tests.ModsManager.Avalonia;

public class ControllerNavigationIntegrationTests
{
    [AvaloniaFact]
    public void SetupMapsEveryAdvancedStorageBrowseButtonToItsField()
    {
        var setup = new SetupWindow();
        var resolver = typeof(SetupWindow).GetMethod(
            "GetPathTextBox",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.Same(
            setup.FindControl<TextBox>("ModStorageTextBox"),
            resolver.Invoke(setup, ["ModStorageTextBox"]));
        Assert.Same(
            setup.FindControl<TextBox>("CollectionStorageTextBox"),
            resolver.Invoke(setup, ["CollectionStorageTextBox"]));
        Assert.Same(
            setup.FindControl<TextBox>("BuiltModsTextBox"),
            resolver.Invoke(setup, ["BuiltModsTextBox"]));
    }

    [AvaloniaFact]
    public void Pcsx2ExtractionConfirmationOnlyDescribesIsoExtraction()
    {
        var root = Path.Combine(Path.GetTempPath(), "OpenKhSetupConfirmationTests", Guid.NewGuid().ToString("N"));
        try
        {
            var layout = InstallationLayout.Detect("ignored", ["--data-root", root]);
            var configuration = new ModManagerConfigurationService(layout);
            configuration.Current.GameEdition = 1;
            var viewModel = new SetupWindowViewModel(configuration);
            var resolver = typeof(SetupWindow).GetMethod(
                "GetExtractionConfirmationDescription",
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var message = Assert.IsType<string>(resolver.Invoke(null, [viewModel]));

            Assert.Contains("ISO files", message);
            Assert.Contains("may be overwritten", message);
            Assert.DoesNotContain("remastered", message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("disk space", message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [AvaloniaFact]
    public void InstallDialogUsesSeparateRepositoryAndLocalFileActions()
    {
        var install = new InstallModWindow();

        Assert.NotNull(install.FindControl<TextBox>("SourceTextBox"));
        Assert.NotNull(install.FindControl<TextBox>("BranchTextBox"));
        Assert.NotNull(install.FindControl<Button>("InstallButton"));
        Assert.NotNull(install.FindControl<Button>("ChooseFileButton"));
        Assert.Null(install.FindControl<CheckBox>("OverwriteCheckBox"));
    }

    [AvaloniaFact]
    public void RepositoryInstallActionAlignsWithTheBranchField()
    {
        var install = new InstallModWindow();
        var window = ShowContent(install, 1500, 900);
        var branch = install.FindControl<TextBox>("BranchTextBox")!;
        var installButton = install.FindControl<Button>("InstallButton")!;

        var branchCenter = GetCenter(branch, install);
        var buttonCenter = GetCenter(installButton, install);

        Assert.InRange(Math.Abs(branchCenter.Y - buttonCenter.Y), 0, 1);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ControllerKeyboardTypesErasesClosesAndRestoresTheDialog()
    {
        var main = new MainWindow();
        var window = ShowContent(main, 1280, 800);
        var install = new InstallModWindow();
        var installTask = main.ShowPageAsync<ModInstallRequest>(install);
        Dispatcher.UIThread.RunJobs();
        var target = install.FindControl<TextBox>("SourceTextBox")!;
        target.Text = "ab";
        target.CaretIndex = 2;
        target.SelectionStart = 2;
        target.SelectionEnd = 2;

        VirtualKeyboardService.Show(target);
        Dispatcher.UIThread.RunJobs();
        var keyboard = main.GetVisualDescendants().OfType<ControllerKeyboardWindow>().Single();

        FocusKeyboardButton(keyboard, "Character:c");
        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.Confirm));
        Assert.Equal("abc", target.Text);

        FocusKeyboardButton(keyboard, "Backspace");
        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.Confirm));
        Assert.Equal("ab", target.Text);

        FocusKeyboardButton(keyboard, "Done");
        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.Confirm));
        await WaitForKeyboardToCloseAsync();

        Assert.False(VirtualKeyboardService.IsOpen);
        Assert.True(install.IsAttachedToVisualTree());
        Dispatcher.UIThread.RunJobs();
        Assert.Same(target, main.FocusManager?.GetFocusedElement());

        install.Close();
        await installTask;
        window.Close();
    }

    [AvaloniaFact]
    public async Task ControllerKeyboardSupportsFaceButtonShortcuts()
    {
        var main = new MainWindow();
        var window = ShowContent(main, 1280, 800);
        var install = new InstallModWindow();
        var installTask = main.ShowPageAsync<ModInstallRequest>(install);
        Dispatcher.UIThread.RunJobs();
        var target = install.FindControl<TextBox>("SourceTextBox")!;
        target.Text = "ab";
        target.CaretIndex = 2;
        target.SelectionStart = 2;
        target.SelectionEnd = 2;

        VirtualKeyboardService.Show(target);
        Dispatcher.UIThread.RunJobs();
        var keyboard = main.GetVisualDescendants().OfType<ControllerKeyboardWindow>().Single();

        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.Secondary));
        Assert.Equal("a", target.Text);

        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.MoveTop));
        FocusKeyboardButton(keyboard, "Character:b");
        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.Confirm));
        Assert.Equal("aB", target.Text);

        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.Cancel));
        await WaitForKeyboardToCloseAsync();
        install.Close();
        await installTask;
        window.Close();
    }

    [AvaloniaFact]
    public async Task PressingEnterSubmitsARepositoryInstall()
    {
        var main = new MainWindow();
        var window = ShowContent(main, 1280, 800);
        var install = new InstallModWindow();
        var installTask = main.ShowPageAsync<ModInstallRequest>(install);
        Dispatcher.UIThread.RunJobs();
        var source = install.FindControl<TextBox>("SourceTextBox")!;
        source.Text = "OpenKH/example-mod";

        source.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Enter,
            Source = source
        });

        var request = await installTask;
        Assert.NotNull(request);
        Assert.Equal("OpenKH/example-mod", request.Source);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ControllerCancelClosesOnlyTheKeyboardAndReturnsControlToTheDialog()
    {
        var main = new MainWindow();
        var window = ShowContent(main, 1280, 800);
        var install = new InstallModWindow();
        var installTask = main.ShowPageAsync<ModInstallRequest>(install);
        Dispatcher.UIThread.RunJobs();
        var target = install.FindControl<TextBox>("SourceTextBox")!;

        VirtualKeyboardService.Show(target);
        Dispatcher.UIThread.RunJobs();
        Assert.True(ControllerWindowNavigator.TryHandleVirtualKeyboard(ControllerAction.Cancel));
        await WaitForKeyboardToCloseAsync();

        Assert.False(VirtualKeyboardService.IsOpen);
        Assert.False(installTask.IsCompleted);
        Assert.True(install.IsAttachedToVisualTree());
        Assert.Same(target, main.FocusManager?.GetFocusedElement());

        install.Close();
        await installTask;
        window.Close();
    }

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
    public void SetupControllerOpensAdvancedStorageAndEntersItsFirstField()
    {
        var setup = new SetupWindow();
        var window = ShowContent(setup, 1500, 1000);
        var expander = setup.FindControl<Expander>("AdvancedStorageExpander")!;
        var firstField = setup.FindControl<TextBox>("ModStorageTextBox")!;
        var headerButton = expander.GetVisualDescendants().OfType<Button>().First();

        Assert.False(expander.IsExpanded);
        Assert.True(headerButton.Focus());

        setup.HandleControllerAction(ControllerAction.Confirm);
        Dispatcher.UIThread.RunJobs();

        Assert.True(expander.IsExpanded);
        Assert.Same(firstField, window.FocusManager?.GetFocusedElement());

        setup.HandleControllerAction(ControllerAction.NavigateRight);
        Dispatcher.UIThread.RunJobs();

        var browseButton = setup.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "ModStorageTextBox"));
        Assert.Same(browseButton, window.FocusManager?.GetFocusedElement());

        setup.HandleControllerAction(ControllerAction.NavigateDown);
        Dispatcher.UIThread.RunJobs();
        var collectionBrowseButton = setup.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "CollectionStorageTextBox"));
        Assert.Same(collectionBrowseButton, window.FocusManager?.GetFocusedElement());

        setup.HandleControllerAction(ControllerAction.NavigateLeft);
        Dispatcher.UIThread.RunJobs();
        var collectionField = setup.FindControl<TextBox>("CollectionStorageTextBox")!;
        Assert.Same(collectionField, window.FocusManager?.GetFocusedElement());

        setup.HandleControllerAction(ControllerAction.NavigateDown);
        Dispatcher.UIThread.RunJobs();
        var builtModsField = setup.FindControl<TextBox>("BuiltModsTextBox")!;
        Assert.Same(builtModsField, window.FocusManager?.GetFocusedElement());

        setup.HandleControllerAction(ControllerAction.NavigateRight);
        Dispatcher.UIThread.RunJobs();
        var builtModsBrowseButton = setup.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "BuiltModsTextBox"));
        Assert.Same(builtModsBrowseButton, window.FocusManager?.GetFocusedElement());
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
    public void MainWindowControllerActivatesMoreCommandsWithoutCollapsingTheMenu()
    {
        var main = new MainWindow();
        var creatorButton = main.FindControl<Button>("CreatorToolsButton")!;
        var aboutButton = main.FindControl<Button>("AboutOpenKhButton")!;
        var moreExpander = main.FindControl<Expander>("MoreExpander")!;
        var creatorInvocations = 0;
        var aboutInvocations = 0;
        creatorButton.Command = new RelayCommand(() => creatorInvocations++);
        aboutButton.Command = new RelayCommand(() => aboutInvocations++);
        moreExpander.IsExpanded = true;
        var window = ShowContent(main, 1280, 800);

        Assert.True(creatorButton.Focus());
        main.HandleControllerAction(ControllerAction.Confirm);
        Assert.Equal(1, creatorInvocations);
        Assert.True(moreExpander.IsExpanded);

        Assert.True(aboutButton.Focus());
        main.HandleControllerAction(ControllerAction.Confirm);
        Assert.Equal(1, aboutInvocations);
        Assert.True(moreExpander.IsExpanded);
        window.Close();
    }

    [AvaloniaFact]
    public void ModReorderingRequiresFocusInsideAModRow()
    {
        var resolver = typeof(MainWindow).GetMethod(
            "TryGetFocusedMod",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var mod = new ModListItemViewModel(
            new ModEntry
            {
                Id = "example/mod",
                Name = "Example Mod",
                Directory = "example"
            },
            () => { });
        var modItem = new ListBoxItem { DataContext = mod };
        object?[] modArguments = [modItem, null];
        object?[] unrelatedArguments = [new Button(), null];

        Assert.True(Assert.IsType<bool>(resolver.Invoke(null, modArguments)));
        Assert.Same(mod, modArguments[1]);
        Assert.False(Assert.IsType<bool>(resolver.Invoke(null, unrelatedArguments)));
        Assert.Null(unrelatedArguments[1]);
    }

    [AvaloniaFact]
    public void ActualMainWindowKeepsAMovedOffscreenModSelected()
    {
        var main = new MainWindow();
        var list = main.FindControl<ListBox>("ModList")!;
        var window = ShowContent(main, 1280, 640);
        var mods = new ObservableCollection<string>(
            Enumerable.Range(0, 40).Select(index => $"Installed mod {index}"));
        list.ItemsSource = mods;
        var selected = mods[^1];
        list.SelectedItem = selected;
        RefreshLayout(window, 1280, 640);

        mods.Remove(selected);
        mods.Insert(0, selected);
        list.SelectedItem = selected;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(selected, list.SelectedItem);
        Assert.Equal(selected, mods[0]);
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

    private static void FocusKeyboardButton(ControllerKeyboardWindow keyboard, string action)
    {
        var button = keyboard.GetVisualDescendants()
            .OfType<Button>()
            .Single(candidate => Equals(candidate.Tag, action));
        Assert.True(button.Focus(NavigationMethod.Directional));
    }

    private static async Task WaitForKeyboardToCloseAsync()
    {
        for (var attempt = 0; attempt < 20 && VirtualKeyboardService.IsOpen; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }
        Dispatcher.UIThread.RunJobs();
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
