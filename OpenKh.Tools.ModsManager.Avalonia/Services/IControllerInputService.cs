namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IControllerInputService : IDisposable
{
    event Action<ControllerAction>? ActionTriggered;
    event Action? ConnectionChanged;

    bool IsConnected { get; }
    string StatusText { get; }
    string NavigationHelpText { get; }
    void Start();
    IDisposable Capture(Action<ControllerAction> handler);
}
