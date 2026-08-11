using System.Globalization;
using Avalonia.Threading;
using Steamworks;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class SteamworksPlatformService : IDisposable
{
    public const uint AppId = 480;

    private DispatcherTimer? _callbackTimer;
    private bool _steamInputInitialized;

    public bool IsRunning { get; private set; }

    public bool TryStart()
    {
        if (IsRunning)
            return true;

        try
        {
            Environment.SetEnvironmentVariable(
                "SteamAppId",
                AppId.ToString(CultureInfo.InvariantCulture));

            if (!SteamAPI.Init())
                return false;

            IsRunning = true;
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
            IsRunning = false;
        }
    }
}
