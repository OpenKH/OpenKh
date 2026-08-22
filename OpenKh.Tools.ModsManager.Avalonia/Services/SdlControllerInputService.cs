using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using SDL3;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class SdlControllerInputService : IControllerInputService
{
    private const short AxisPressThreshold = 18000;
    private const short AxisReleaseThreshold = 9000;
    private const int AxisInitialRepeatDelayMilliseconds = 320;
    private const int AxisRepeatIntervalMilliseconds = 90;
    private readonly DispatcherTimer _pollTimer;
    private readonly Dictionary<uint, IntPtr> _gamepads = [];
    private readonly Dictionary<uint, XInputState> _xInputStates = [];
    private readonly HashSet<SDL.GamepadButton> _pressedButtons = [];
    private int _horizontalAxisDirection;
    private int _verticalAxisDirection;
    private int _rightVerticalAxisDirection;
    private bool _leftTriggerPressed;
    private bool _rightTriggerPressed;
    private long _horizontalAxisNextRepeat;
    private long _verticalAxisNextRepeat;
    private long _rightVerticalAxisNextRepeat;
    private int _discoveryTicks;
    private bool _initialized;
    private bool _xInputAvailable = OperatingSystem.IsWindows();
    private string _statusText = "No controller detected";
    private string _navigationHelpText = "";
    private Action<ControllerAction>? _capturedHandler;

    public SdlControllerInputService()
    {
        SetDisconnectedStatus();
        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _pollTimer.Tick += (_, _) => PollEvents();
    }

    public event Action<ControllerAction>? ActionTriggered;
    public event Action? ConnectionChanged;
    public event Action? StatusChanged;

    public bool IsConnected => _gamepads.Count > 0 || _xInputStates.Count > 0;
    public string StatusText => _statusText;
    public string NavigationHelpText => _navigationHelpText;

    public void Start()
    {
        try
        {
            SDL.SetHint("SDL_JOYSTICK_HIDAPI", "1");
            SDL.SetHint("SDL_JOYSTICK_HIDAPI_STEAM", "1");
            SDL.SetHint("SDL_JOYSTICK_HIDAPI_STEAMDECK", "1");
            SDL.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "0");
            _initialized = SDL.Init(SDL.InitFlags.Gamepad);
            if (!_initialized)
            {
                _statusText = $"Controller support unavailable: {SDL.GetError()}";
            }
            else
            {
                RefreshConnectedGamepads();
            }
            _pollTimer.Start();
            PollEvents();
        }
        catch (Exception exception) when (
            exception is DllNotFoundException or EntryPointNotFoundException or TypeInitializationException)
        {
            _statusText = "Controller support unavailable";
            if (OperatingSystem.IsWindows())
            {
                _pollTimer.Start();
                PollEvents();
            }
            ConnectionChanged?.Invoke();
            StatusChanged?.Invoke();
        }
    }

    private void RefreshConnectedGamepads()
    {
        var gamepadIds = SDL.GetGamepads(out var count);
        if (gamepadIds is null)
            return;

        var connectedIds = gamepadIds.Take(count).ToHashSet();
        foreach (var removedId in _gamepads.Keys.Except(connectedIds).ToArray())
            RemoveGamepad(removedId);
        foreach (var instanceId in connectedIds)
        {
            if (_gamepads.TryGetValue(instanceId, out var gamepad) &&
                HasGamepadIdentityChanged(instanceId, gamepad))
            {
                RemoveGamepad(instanceId);
            }

            AddGamepad(instanceId);
        }

        var previousStatus = _statusText;
        var previousNavigationHelp = _navigationHelpText;
        UpdateControllerStatus();
        if (!string.Equals(previousStatus, _statusText, StringComparison.Ordinal) ||
            !string.Equals(previousNavigationHelp, _navigationHelpText, StringComparison.Ordinal))
        {
            StatusChanged?.Invoke();
        }
    }

    private static bool HasGamepadIdentityChanged(uint instanceId, IntPtr gamepad)
    {
        if (!SDL.GamepadConnected(gamepad))
            return true;

        var currentName = SDL.GetGamepadName(gamepad) ?? string.Empty;
        var detectedName = SDL.GetGamepadNameForID(instanceId) ?? string.Empty;
        return SDL.GetGamepadVendor(gamepad) != SDL.GetGamepadVendorForID(instanceId) ||
               SDL.GetGamepadProduct(gamepad) != SDL.GetGamepadProductForID(instanceId) ||
               !string.Equals(currentName, detectedName, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _pollTimer.Stop();
        foreach (var gamepad in _gamepads.Values)
            SDL.CloseGamepad(gamepad);
        _gamepads.Clear();
        _xInputStates.Clear();

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
        if (_initialized)
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
                RefreshConnectedGamepads();
            }
        }

        if (!PollXInputState())
            PollGamepadState();
    }

    private bool PollXInputState()
    {
        if (!_xInputAvailable || !OperatingSystem.IsWindows())
            return false;

        var connectionChanged = false;
        try
        {
            for (uint index = 0; index < 4; index++)
            {
                if (XInputGetState(index, out var currentState) != 0)
                {
                    connectionChanged |= _xInputStates.Remove(index);
                    continue;
                }

                if (!_xInputStates.TryGetValue(index, out var previousState))
                {
                    _xInputStates[index] = currentState;
                    connectionChanged = true;
                    continue;
                }

                var pressedButtons = (ushort)(currentState.Gamepad.Buttons & ~previousState.Gamepad.Buttons);
                PollXInputButtons(pressedButtons);
                HandleAxis(SDL.GamepadAxis.LeftY, (short)-currentState.Gamepad.ThumbLY);
                HandleAxis(SDL.GamepadAxis.LeftX, currentState.Gamepad.ThumbLX);
                HandleAxis(SDL.GamepadAxis.RightY, (short)-currentState.Gamepad.ThumbRY);
                HandleTrigger(
                    currentState.Gamepad.LeftTrigger * 128,
                    ref _leftTriggerPressed,
                    ControllerAction.MoveUp);
                HandleTrigger(
                    currentState.Gamepad.RightTrigger * 128,
                    ref _rightTriggerPressed,
                    ControllerAction.MoveDown);
                _xInputStates[index] = currentState;
            }
        }
        catch (DllNotFoundException)
        {
            _xInputAvailable = false;
            _xInputStates.Clear();
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            _xInputAvailable = false;
            _xInputStates.Clear();
            return false;
        }

        if (connectionChanged)
        {
            UpdateControllerStatus();
            ConnectionChanged?.Invoke();
            StatusChanged?.Invoke();
        }

        return _xInputStates.Count > 0;
    }

    private void PollXInputButtons(ushort pressedButtons)
    {
        DispatchXInputButton(pressedButtons, 0x0001, ControllerAction.NavigateUp);
        DispatchXInputButton(pressedButtons, 0x0002, ControllerAction.NavigateDown);
        DispatchXInputButton(pressedButtons, 0x0004, ControllerAction.NavigateLeft);
        DispatchXInputButton(pressedButtons, 0x0008, ControllerAction.NavigateRight);
        DispatchXInputButton(pressedButtons, 0x0010, ControllerAction.Refresh);
        DispatchXInputButton(pressedButtons, 0x1000, ControllerAction.Confirm);
        DispatchXInputButton(pressedButtons, 0x2000, ControllerAction.Cancel);
        DispatchXInputButton(pressedButtons, 0x4000, ControllerAction.Secondary);
        DispatchXInputButton(pressedButtons, 0x8000, ControllerAction.MoveTop);
        DispatchXInputButton(pressedButtons, 0x0100, ControllerAction.PreviousGame);
        DispatchXInputButton(pressedButtons, 0x0200, ControllerAction.NextGame);
    }

    private void DispatchXInputButton(ushort pressedButtons, ushort mask, ControllerAction action)
    {
        if ((pressedButtons & mask) != 0)
            Dispatch(action);
    }

    private void PollGamepadState()
    {
        if (_gamepads.Count == 0)
            return;

        SDL.UpdateGamepads();
        var gamepad = _gamepads.Values.First();
        PollButton(gamepad, SDL.GamepadButton.DPadUp);
        PollButton(gamepad, SDL.GamepadButton.DPadDown);
        PollButton(gamepad, SDL.GamepadButton.DPadLeft);
        PollButton(gamepad, SDL.GamepadButton.DPadRight);
        PollButton(gamepad, SDL.GamepadButton.South);
        PollButton(gamepad, SDL.GamepadButton.East);
        PollButton(gamepad, SDL.GamepadButton.West);
        PollButton(gamepad, SDL.GamepadButton.North);
        PollButton(gamepad, SDL.GamepadButton.LeftShoulder);
        PollButton(gamepad, SDL.GamepadButton.RightShoulder);
        PollButton(gamepad, SDL.GamepadButton.Start);
        HandleAxis(SDL.GamepadAxis.LeftY, SDL.GetGamepadAxis(gamepad, SDL.GamepadAxis.LeftY));
        HandleAxis(SDL.GamepadAxis.LeftX, SDL.GetGamepadAxis(gamepad, SDL.GamepadAxis.LeftX));
        HandleAxis(SDL.GamepadAxis.RightY, SDL.GetGamepadAxis(gamepad, SDL.GamepadAxis.RightY));
        HandleTrigger(
            SDL.GetGamepadAxis(gamepad, SDL.GamepadAxis.LeftTrigger),
            ref _leftTriggerPressed,
            ControllerAction.MoveUp);
        HandleTrigger(
            SDL.GetGamepadAxis(gamepad, SDL.GamepadAxis.RightTrigger),
            ref _rightTriggerPressed,
            ControllerAction.MoveDown);
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
        var vendor = SDL.GetGamepadVendor(gamepad);
        _statusText = $"Controller connected: {name}";
        _navigationHelpText = GetNavigationHelpText(name, vendor);
        ConnectionChanged?.Invoke();
        StatusChanged?.Invoke();
    }

    private void RemoveGamepad(uint instanceId)
    {
        if (_gamepads.Remove(instanceId, out var gamepad))
            SDL.CloseGamepad(gamepad);

        UpdateControllerStatus();
        if (!IsConnected)
        {
            _pressedButtons.Clear();
            _horizontalAxisDirection = 0;
            _verticalAxisDirection = 0;
            _rightVerticalAxisDirection = 0;
            _horizontalAxisNextRepeat = 0;
            _verticalAxisNextRepeat = 0;
            _rightVerticalAxisNextRepeat = 0;
            _leftTriggerPressed = false;
            _rightTriggerPressed = false;
        }
        ConnectionChanged?.Invoke();
        StatusChanged?.Invoke();
    }

    private void HandleButton(SDL.GamepadButton button)
    {
        var action = button switch
        {
            SDL.GamepadButton.DPadUp => ControllerAction.NavigateUp,
            SDL.GamepadButton.DPadDown => ControllerAction.NavigateDown,
            SDL.GamepadButton.DPadLeft => ControllerAction.NavigateLeft,
            SDL.GamepadButton.DPadRight => ControllerAction.NavigateRight,
            SDL.GamepadButton.South => ControllerAction.Confirm,
            SDL.GamepadButton.East => ControllerAction.Cancel,
            SDL.GamepadButton.West => ControllerAction.Secondary,
            SDL.GamepadButton.North => ControllerAction.MoveTop,
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
            HandleAxisDirection(
                value,
                ref _verticalAxisDirection,
                ref _verticalAxisNextRepeat,
                ControllerAction.NavigateUp,
                ControllerAction.NavigateDown);
        }
        else if (axis == SDL.GamepadAxis.LeftX)
        {
            HandleAxisDirection(
                value,
                ref _horizontalAxisDirection,
                ref _horizontalAxisNextRepeat,
                ControllerAction.NavigateLeft,
                ControllerAction.NavigateRight);
        }
        else if (axis == SDL.GamepadAxis.RightY)
        {
            HandleAxisDirection(
                value,
                ref _rightVerticalAxisDirection,
                ref _rightVerticalAxisNextRepeat,
                ControllerAction.ScrollUp,
                ControllerAction.ScrollDown);
        }
    }

    private void HandleAxisDirection(
        short value,
        ref int currentDirection,
        ref long nextRepeat,
        ControllerAction negativeAction,
        ControllerAction positiveAction)
    {
        var magnitude = Math.Abs((int)value);
        if (magnitude < AxisReleaseThreshold)
        {
            currentDirection = 0;
            nextRepeat = 0;
            return;
        }

        if (magnitude < AxisPressThreshold)
            return;

        var direction = value < 0 ? -1 : 1;
        var now = Environment.TickCount64;
        if (currentDirection != direction)
        {
            currentDirection = direction;
            nextRepeat = now + AxisInitialRepeatDelayMilliseconds;
            Dispatch(direction < 0 ? negativeAction : positiveAction);
            return;
        }

        if (now < nextRepeat)
            return;

        nextRepeat = now + AxisRepeatIntervalMilliseconds;
        Dispatch(direction < 0 ? negativeAction : positiveAction);
    }

    private void HandleTrigger(int value, ref bool isPressed, ControllerAction action)
    {
        if (value < AxisReleaseThreshold)
        {
            isPressed = false;
            return;
        }

        if (value < AxisPressThreshold || isPressed)
            return;

        isPressed = true;
        Dispatch(action);
    }

    public void Dispatch(ControllerAction action)
    {
        if (!IsApplicationActive())
            return;

        if (_capturedHandler is { } handler)
            handler(action);
        else
            ActionTriggered?.Invoke(action);
    }

    private static bool IsApplicationActive()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.Windows.Count == 0)
        {
            return true;
        }

        return desktop.Windows.Any(window => window.IsActive);
    }

    private static string GetNavigationHelpText(string gamepadName, ushort vendor)
    {
        if (vendor == 0x054c ||
            gamepadName.Contains("DualSense", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("DualShock", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("PlayStation", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("PS4", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("PS5", StringComparison.OrdinalIgnoreCase))
        {
            return "D-pad / left stick: navigate   Right stick: scroll   Cross: select   Circle: back   Square: open folder   Triangle: move to top   L2/R2: move mod   L1/R1: game   Options: updates";
        }

        if (vendor == 0x057e ||
            gamepadName.Contains("Nintendo", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("Switch", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("Joy-Con", StringComparison.OrdinalIgnoreCase))
        {
            return "D-pad / left stick: navigate   Right stick: scroll   B: select   A: back   Y: open folder   X: move to top   ZL/ZR: move mod   L/R: game   Plus: updates";
        }

        if (vendor == 0x28de ||
            gamepadName.Contains("Steam Deck", StringComparison.OrdinalIgnoreCase) ||
            gamepadName.Contains("Steam Virtual", StringComparison.OrdinalIgnoreCase))
        {
            return "D-pad / left stick: navigate   Right stick: scroll   A: select   B: back   X: open folder   Y: move to top   L2/R2: move mod   L1/R1: game   Menu: updates   Steam + X: keyboard";
        }

        return "D-pad / left stick: navigate   Right stick: scroll   A: select   B: back   X: open folder   Y: move to top   LT/RT: move mod   LB/RB: game   Menu: updates";
    }

    private void SetDisconnectedStatus()
    {
        if (IsSteamDeckEnvironment())
        {
            _statusText = "Steam Deck detected, waiting for controller input";
            _navigationHelpText = "Open the app from its Steam shortcut. Use Steam + X for the keyboard.";
            return;
        }

        _statusText = "No controller detected";
        _navigationHelpText = string.Empty;
    }

    private void UpdateControllerStatus()
    {
        if (_gamepads.Count > 0)
        {
            var gamepad = _gamepads.Values.First();
            var name = SDL.GetGamepadName(gamepad) ?? "Gamepad";
            _statusText = $"Controller connected: {name}";
            _navigationHelpText = GetNavigationHelpText(name, SDL.GetGamepadVendor(gamepad));
            return;
        }

        if (_xInputStates.Count > 0)
        {
            _statusText = "Controller connected: Xbox controller";
            _navigationHelpText = GetNavigationHelpText("Xbox controller", 0x045e);
            return;
        }

        SetDisconnectedStatus();
    }

    private static bool IsSteamDeckEnvironment()
    {
        if (!OperatingSystem.IsLinux())
            return false;

        if (Environment.GetEnvironmentVariable("SteamDeck") == "1" ||
            Environment.GetEnvironmentVariable("SteamGamepadUI") == "1")
            return true;

        try
        {
            return File.Exists("/etc/os-release") &&
                   File.ReadAllText("/etc/os-release")
                       .Contains("steamos", StringComparison.OrdinalIgnoreCase);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern uint XInputGetState(uint userIndex, out XInputState state);

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputState
    {
        public uint PacketNumber;
        public XInputGamepad Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputGamepad
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short ThumbLX;
        public short ThumbLY;
        public short ThumbRX;
        public short ThumbRY;
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
