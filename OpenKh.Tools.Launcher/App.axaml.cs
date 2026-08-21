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

    internal IControllerInputService Controller => _controller;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        InputElement.KeyDownEvent.AddClassHandler<Window>(HandleGlobalKeyDown, RoutingStrategies.Tunnel);
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
            _controller.Start();

            var mainWindow = new MainWindow(_controller);
            _controller.ActionTriggered += mainWindow.HandleControllerAction;
            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) =>
            {
                _controller.ActionTriggered -= mainWindow.HandleControllerAction;
                _controller.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void HandleGlobalKeyDown(Window window, KeyEventArgs eventArgs)
    {
        if (eventArgs.KeyModifiers != KeyModifiers.None)
            return;

        if (eventArgs.Source is TextBox && eventArgs.Key is not Key.Escape)
            return;

        var action = eventArgs.Key switch
        {
            Key.Up => ControllerAction.NavigateUp,
            Key.Down => ControllerAction.NavigateDown,
            Key.Left => ControllerAction.NavigateLeft,
            Key.Right => ControllerAction.NavigateRight,
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
