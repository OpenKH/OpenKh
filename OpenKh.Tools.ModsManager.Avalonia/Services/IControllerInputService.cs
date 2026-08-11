namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IControllerInputService : IDisposable
{
    event Action<ControllerAction>? ActionTriggered;
    event Action? ConnectionChanged;
    event Action? StatusChanged;

    bool IsConnected { get; }
    string StatusText { get; }
    string NavigationHelpText { get; }
    void Start();
    void Dispatch(ControllerAction action);
    void SetSteamInputFallback(bool enabled, bool isSteamDeck);
    IDisposable Capture(Action<ControllerAction> handler);
}
