using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.Launcher;

public partial class App : Application
{
    private readonly IControllerInputService _controller = new SdlControllerInputService();
    private SteamworksPlatformService? _steamworks;

    internal IControllerInputService Controller => _controller;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        InputElement.KeyDownEvent.AddClassHandler<Window>(HandleGlobalKeyDown, RoutingStrategies.Tunnel);
        InputElement.GotFocusEvent.AddClassHandler<TextBox>(HandleTextBoxFocus, RoutingStrategies.Bubble);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

            if (LegacyInstallationMigration.TryStartModManager())
            {
                if (!string.IsNullOrWhiteSpace(LegacyInstallationMigration.LastError))
                    desktop.MainWindow = new MessageDialog("OpenKH Update", LegacyInstallationMigration.LastError!);
                else
                    desktop.Shutdown();

                base.OnFrameworkInitializationCompleted();
                return;
            }

            LegacyInstallationMigration.ScheduleCleanupIfNeeded();
            _steamworks = new SteamworksPlatformService();
            _steamworks.TryStart();
            _controller.Start();
            _controller.ConnectionChanged += HandleControllerConnectionChanged;
            UpdateSteamLauncherMode();

            var mainWindow = new MainWindow(_controller, _steamworks);
            _controller.ActionTriggered += mainWindow.HandleControllerAction;
            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) =>
            {
                _controller.ActionTriggered -= mainWindow.HandleControllerAction;
                _controller.ConnectionChanged -= HandleControllerConnectionChanged;
                _controller.Dispose();
                _steamworks?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void HandleControllerConnectionChanged() =>
        UpdateSteamLauncherMode();

    private void UpdateSteamLauncherMode()
    {
        var useSteamInput = !_controller.IsConnected && _steamworks?.IsRunning == true;
        _steamworks?.SetLauncherMode(useSteamInput);
        _controller.SetSteamInputFallback(useSteamInput, _steamworks?.IsSteamDeck == true);
    }

    private void HandleTextBoxFocus(TextBox textBox, FocusChangedEventArgs eventArgs) =>
        _steamworks?.ShowFloatingKeyboard(textBox);

    private void HandleGlobalKeyDown(Window window, KeyEventArgs eventArgs)
    {
        if (_steamworks?.IsLauncherModeEnabled != true)
            return;

        if (eventArgs.Source is TextBox && eventArgs.Key is not Key.Escape)
            return;

        var action = eventArgs.Key switch
        {
            Key.Up => ControllerAction.PreviousControl,
            Key.Down => ControllerAction.NextControl,
            Key.Left => ControllerAction.PreviousGame,
            Key.Right => ControllerAction.NextGame,
            Key.Enter or Key.Space => ControllerAction.Confirm,
            Key.Escape => ControllerAction.Cancel,
            Key.F5 => ControllerAction.Refresh,
            _ => (ControllerAction?)null,
        };

        if (action is null)
            return;

        _controller.Dispatch(action.Value);
        eventArgs.Handled = true;
    }
}
