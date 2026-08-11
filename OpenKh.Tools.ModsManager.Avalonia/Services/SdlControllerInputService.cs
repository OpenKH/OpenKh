using Avalonia.Threading;
using SDL3;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class SdlControllerInputService : IControllerInputService
{
    private const short AxisPressThreshold = 18000;
    private const short AxisReleaseThreshold = 9000;
    private readonly DispatcherTimer _pollTimer;
    private readonly Dictionary<uint, IntPtr> _gamepads = [];
    private readonly HashSet<SDL.GamepadButton> _pressedButtons = [];
    private bool _horizontalAxisPressed;
    private bool _verticalAxisPressed;
    private int _discoveryTicks;
    private bool _initialized;
    private string _statusText = "No controller detected";
    private string _navigationHelpText = "";
    private Action<ControllerAction>? _capturedHandler;

    public SdlControllerInputService()
    {
        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _pollTimer.Tick += (_, _) => PollEvents();
    }

    public event Action<ControllerAction>? ActionTriggered;
    public event Action? ConnectionChanged;

    public bool IsConnected => _gamepads.Count > 0;
    public string StatusText => _statusText;
    public string NavigationHelpText => _navigationHelpText;

    public void Start()
    {
        try
        {
            _initialized = SDL.Init(SDL.InitFlags.Gamepad);
            if (!_initialized)
            {
                _statusText = $"Controller support unavailable: {SDL.GetError()}";
                ConnectionChanged?.Invoke();
                return;
            }

            OpenConnectedGamepads();
            _pollTimer.Start();
            PollEvents();
        }
        catch (Exception exception) when (
            exception is DllNotFoundException or EntryPointNotFoundException or TypeInitializationException)
        {
            _statusText = "Controller support unavailable, Steam Input keyboard mapping is still supported";
            ConnectionChanged?.Invoke();
        }
    }

    private void OpenConnectedGamepads()
    {
        var gamepadIds = SDL.GetGamepads(out var count);
        if (gamepadIds is null)
            return;
        foreach (var instanceId in gamepadIds.Take(count))
            AddGamepad(instanceId);
    }

    public void Dispose()
    {
        _pollTimer.Stop();
        foreach (var gamepad in _gamepads.Values)
            SDL.CloseGamepad(gamepad);
        _gamepads.Clear();

        if (_initialized)
            SDL.QuitSubSystem(SDL.InitFlags.Gamepad);
    }

    public IDisposable Capture(Action<ControllerAction> handler)
    {
        var previousHandler = _capturedHandler;
        _capturedHandler = handler;
        return new CaptureScope(this, handler, previousHandler);
    }

    private void PollEvents()
    {
        while (SDL.PollEvent(out var controllerEvent))
        {
            switch ((SDL.EventType)controllerEvent.Type)
            {
                case SDL.EventType.GamepadAdded:
                    AddGamepad(controllerEvent.GDevice.Which);
                    break;
                case SDL.EventType.GamepadRemoved:
                    RemoveGamepad(controllerEvent.GDevice.Which);
                    break;
            }
        }

        if (++_discoveryTicks >= 60)
        {
            _discoveryTicks = 0;
            OpenConnectedGamepads();
        }

        PollGamepadState();
    }

    private void PollGamepadState()
    {
        if (_gamepads.Count == 0)
            return;

        SDL.UpdateGamepads();
        var gamepad = _gamepads.Values.First();
        PollButton(gamepad, SDL.GamepadButton.DPadUp);
        PollButton(gamepad, SDL.GamepadButton.DPadDown);
        PollButton(gamepad, SDL.GamepadButton.South);
        PollButton(gamepad, SDL.GamepadButton.East);
        PollButton(gamepad, SDL.GamepadButton.West);
        PollButton(gamepad, SDL.GamepadButton.North);
        PollButton(gamepad, SDL.GamepadButton.LeftShoulder);
        PollButton(gamepad, SDL.GamepadButton.RightShoulder);
        PollButton(gamepad, SDL.GamepadButton.Start);
        HandleAxis(SDL.GamepadAxis.LeftY, SDL.GetGamepadAxis(gamepad, SDL.GamepadAxis.LeftY));
        HandleAxis(SDL.GamepadAxis.LeftX, SDL.GetGamepadAxis(gamepad, SDL.GamepadAxis.LeftX));
    }

    private void PollButton(IntPtr gamepad, SDL.GamepadButton button)
    {
        var isPressed = SDL.GetGamepadButton(gamepad, button);
        if (!isPressed)
        {
            _pressedButtons.Remove(button);
            return;
        }

        if (_pressedButtons.Add(button))
            HandleButton(button);
    }

    private void AddGamepad(uint instanceId)
    {
        if (_gamepads.ContainsKey(instanceId))
            return;

        var gamepad = SDL.OpenGamepad(instanceId);
        if (gamepad == IntPtr.Zero)
            return;

        _gamepads[instanceId] = gamepad;
        var name = SDL.GetGamepadName(gamepad) ?? "Gamepad";
        _statusText = $"Controller connected: {name}";
        _navigationHelpText = GetNavigationHelpText(name);
        ConnectionChanged?.Invoke();
    }

    private void RemoveGamepad(uint instanceId)
    {
        if (_gamepads.Remove(instanceId, out var gamepad))
            SDL.CloseGamepad(gamepad);

        if (IsConnected)
        {
            var remainingGamepad = _gamepads.Values.First();
            var name = SDL.GetGamepadName(remainingGamepad) ?? "Gamepad";
            _statusText = $"Controller connected: {name}";
            _navigationHelpText = GetNavigationHelpText(name);
        }
        else
        {
            _pressedButtons.Clear();
            _horizontalAxisPressed = false;
            _verticalAxisPressed = false;
            _statusText = "No controller detected";
            _navigationHelpText = "";
        }
        ConnectionChanged?.Invoke();
    }

    private void HandleButton(SDL.GamepadButton button)
    {
        var action = button switch
        {
            SDL.GamepadButton.DPadUp => ControllerAction.PreviousControl,
            SDL.GamepadButton.DPadDown => ControllerAction.NextControl,
            SDL.GamepadButton.South => ControllerAction.Confirm,
            SDL.GamepadButton.East => ControllerAction.Cancel,
            SDL.GamepadButton.West => ControllerAction.Secondary,
            SDL.GamepadButton.North => ControllerAction.Install,
            SDL.GamepadButton.LeftShoulder => ControllerAction.PreviousGame,
            SDL.GamepadButton.RightShoulder => ControllerAction.NextGame,
            SDL.GamepadButton.Start => ControllerAction.Refresh,
            _ => (ControllerAction?)null
        };

        if (action.HasValue)
            Dispatch(action.Value);
    }

    private void HandleAxis(SDL.GamepadAxis axis, short value)
    {
        if (axis == SDL.GamepadAxis.LeftY)
        {
            if (Math.Abs(value) < AxisReleaseThreshold)
            {
                _verticalAxisPressed = false;
                return;
            }

            if (_verticalAxisPressed || Math.Abs(value) < AxisPressThreshold)
                return;

            _verticalAxisPressed = true;
            Dispatch(value < 0
                ? ControllerAction.PreviousItem
                : ControllerAction.NextItem);
        }
        else if (axis == SDL.GamepadAxis.LeftX)
        {
            if (Math.Abs(value) < AxisReleaseThreshold)
            {
                _horizontalAxisPressed = false;
                return;
            }

            if (_horizontalAxisPressed || Math.Abs(value) < AxisPressThreshold)
                return;

            _horizontalAxisPressed = true;
            Dispatch(value < 0
                ? ControllerAction.PreviousGame
                : ControllerAction.NextGame);
        }
    }

    private void Dispatch(ControllerAction action)
    {
        if (_capturedHandler is { } handler)
            handler(action);
        else
            ActionTriggered?.Invoke(action);
    }

    private static string GetNavigationHelpText(string gamepadName)
    {
        if (gamepadName.Contains("DualSense", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("DualShock", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("PlayStation", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("PS4", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("PS5", StringComparison.OrdinalIgnoreCase))
        {
            return "D-pad: navigate   Left stick: mods   Cross: select   Circle: back   Square: open folder   Triangle: install   L1/R1: game   Options: updates";
        }

        if (gamepadName.Contains("Nintendo", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("Switch", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("Joy-Con", StringComparison.OrdinalIgnoreCase))
        {
            return "D-pad: navigate   Left stick: mods   B: select   A: back   Y: open folder   X: install   L/R: game   Plus: updates";
        }

        return "D-pad: navigate   Left stick: mods   A: select   B: back   X: open folder   Y: install   LB/RB: game   Menu: updates";
    }

    private sealed class CaptureScope(
        SdlControllerInputService owner,
        Action<ControllerAction> handler,
        Action<ControllerAction>? previousHandler) : IDisposable
    {
        public void Dispose()
        {
            if (owner._capturedHandler == handler)
                owner._capturedHandler = previousHandler;
        }
    }
}
