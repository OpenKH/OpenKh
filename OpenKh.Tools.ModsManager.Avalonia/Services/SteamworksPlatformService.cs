using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Steamworks;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class SteamworksPlatformService : IDisposable
{
    public const uint AppId = 480;

    private DispatcherTimer? _callbackTimer;
    private bool _steamInputInitialized;

    public bool IsRunning { get; private set; }
    public bool IsSteamDeck { get; private set; }
    public bool IsLauncherModeEnabled { get; private set; }

    public bool TryStart()
    {
        if (IsRunning)
            return true;

        try
        {
            Environment.SetEnvironmentVariable(
                "SteamAppId",
                AppId.ToString(CultureInfo.InvariantCulture));
            Environment.SetEnvironmentVariable(
                "SteamGameId",
                AppId.ToString(CultureInfo.InvariantCulture));

            if (!SteamAPI.Init())
                return false;

            IsRunning = true;
            IsSteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
            _steamInputInitialized = SteamInput.Init(false);
            _callbackTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Input, PumpCallbacks);
            _callbackTimer.Start();
            return true;
        }
        catch (Exception)
        {
            Dispose();
            return false;
        }
    }

    public void SetLauncherMode(bool enabled)
    {
        if (!IsRunning || IsLauncherModeEnabled == enabled)
            return;

        try
        {
            SteamUtils.SetGameLauncherMode(enabled);
            IsLauncherModeEnabled = enabled;
        }
        catch (Exception)
        {
            IsLauncherModeEnabled = false;
        }
    }

    public bool ShowFloatingKeyboard(TextBox textBox)
    {
        if (!IsRunning || !IsSteamDeck || TopLevel.GetTopLevel(textBox) is not { } topLevel)
            return false;

        var origin = textBox.TranslatePoint(new Point(0, 0), topLevel) ?? new Point(0, 0);
        var scale = topLevel.RenderScaling;
        var keyboardMode = textBox.AcceptsReturn
            ? EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeMultipleLines
            : EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine;

        try
        {
            return SteamUtils.ShowFloatingGamepadTextInput(
                keyboardMode,
                (int)Math.Round(origin.X * scale),
                (int)Math.Round(origin.Y * scale),
                Math.Max(1, (int)Math.Round(textBox.Bounds.Width * scale)),
                Math.Max(1, (int)Math.Round(textBox.Bounds.Height * scale)));
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void PumpCallbacks(object? sender, EventArgs eventArgs)
    {
        if (!IsRunning)
            return;

        try
        {
            SteamAPI.RunCallbacks();
            if (_steamInputInitialized)
                SteamInput.RunFrame(false);
        }
        catch (Exception)
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        _callbackTimer?.Stop();
        _callbackTimer = null;

        if (!IsRunning)
            return;

        try
        {
            if (IsLauncherModeEnabled)
                SteamUtils.SetGameLauncherMode(false);
            if (_steamInputInitialized)
                SteamInput.Shutdown();
            SteamAPI.Shutdown();
        }
        catch (Exception)
        {
        }
        finally
        {
            _steamInputInitialized = false;
            IsLauncherModeEnabled = false;
            IsSteamDeck = false;
            IsRunning = false;
        }
    }
}
