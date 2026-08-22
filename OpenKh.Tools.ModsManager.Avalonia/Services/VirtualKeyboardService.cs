using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IVirtualKeyboardHost
{
    bool IsOpen { get; }
    void Show(TextBox textBox);
    bool HandleControllerAction(ControllerAction action);
    bool Hide();
}

public static class VirtualKeyboardService
{
    private const string SteamKeyboardUri =
        "steam://open/keyboard?XPosition=0&YPosition=0&Width=0&Height=0&Mode=0";
    private const string SteamCloseKeyboardUri = "steam://close/keyboard";
    private const uint WindowSystemCommand = 0x0112;
    private const uint SystemCommandClose = 0xF060;
    private static IVirtualKeyboardHost? _host;
    private static KeyboardBackend _backend;
    private static WeakReference<TextBox>? _target;

    public static bool IsOpen => _host?.IsOpen ?? _backend != KeyboardBackend.None;

    public static void Configure(IVirtualKeyboardHost host)
    {
        _host = host;
    }

    public static void Show(TextBox textBox)
    {
        if (_host is not null)
        {
            _host.Show(textBox);
            return;
        }

        textBox.Focus(NavigationMethod.Directional);
        _target = new WeakReference<TextBox>(textBox);

        try
        {
            if (ShouldUseSteamKeyboard())
            {
                OpenSteamUri(SteamKeyboardUri);
                _backend = KeyboardBackend.Steam;
                return;
            }

            if (OperatingSystem.IsWindows())
            {
                ShowWindowsKeyboard();
                _backend = KeyboardBackend.Windows;
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                OpenSteamUri(SteamKeyboardUri);
                _backend = KeyboardBackend.Steam;
            }
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
    }

    public static bool HandleControllerAction(ControllerAction action)
    {
        if (_host is not null)
            return _host.HandleControllerAction(action);
        if (_backend == KeyboardBackend.None)
            return false;

        if (action == ControllerAction.Cancel)
            Hide();

        return true;
    }

    public static bool Hide()
    {
        if (_host is not null)
            return _host.Hide();
        if (_backend == KeyboardBackend.None)
            return false;

        try
        {
            if (_backend == KeyboardBackend.Windows)
                HideWindowsKeyboard();
            else if (_backend == KeyboardBackend.Steam)
                OpenSteamUri(SteamCloseKeyboardUri);
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
        finally
        {
            _backend = KeyboardBackend.None;
            if (_target?.TryGetTarget(out var textBox) == true)
                textBox.Focus(NavigationMethod.Directional);
            _target = null;
        }

        return true;
    }

    private static bool ShouldUseSteamKeyboard()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return false;

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SteamAppId")) ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SteamGameId")) ||
            Environment.GetEnvironmentVariable("SteamDeck") == "1" ||
            Environment.GetEnvironmentVariable("SteamGamepadUI") == "1")
        {
            return true;
        }

        try
        {
            var processes = Process.GetProcessesByName("steam");
            try
            {
                return processes.Length > 0;
            }
            finally
            {
                foreach (var process in processes)
                    process.Dispose();
            }
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or
            System.ComponentModel.Win32Exception or
            PlatformNotSupportedException)
        {
            return false;
        }
    }

    private static void ShowWindowsKeyboard()
    {
        var commonProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
        var touchKeyboard = Path.Combine(
            commonProgramFiles,
            "microsoft shared",
            "ink",
            "TabTip.exe");
        var executable = File.Exists(touchKeyboard) ? touchKeyboard : "osk.exe";
        Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = true
        });
    }

    private static void OpenSteamUri(string uri)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "steam",
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(uri);
        Process.Start(startInfo);
    }

    private static void HideWindowsKeyboard()
    {
        var touchKeyboardWindow = FindWindow("IPTip_Main_Window", null);
        if (touchKeyboardWindow != IntPtr.Zero)
            PostMessage(touchKeyboardWindow, WindowSystemCommand, (IntPtr)SystemCommandClose, IntPtr.Zero);

        foreach (var process in Process.GetProcessesByName("osk"))
        {
            using (process)
                process.CloseMainWindow();
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? className, string? windowName);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr window, uint message, IntPtr wordParameter, IntPtr longParameter);

    private enum KeyboardBackend
    {
        None,
        Windows,
        Steam
    }
}
