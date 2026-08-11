using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public static class VirtualKeyboardService
{
    private const string SteamKeyboardUri =
        "steam://open/keyboard?XPosition=0&YPosition=0&Width=0&Height=0&Mode=0";
    private const string SteamCloseKeyboardUri = "steam://close/keyboard";
    private const uint WindowSystemCommand = 0x0112;
    private const uint SystemCommandClose = 0xF060;
    private static bool _isOpen;

    public static void Show(TextBox textBox)
    {
        textBox.Focus(NavigationMethod.Directional);

        try
        {
            if (OperatingSystem.IsWindows())
            {
                ShowWindowsKeyboard();
                _isOpen = true;
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                ShowSteamKeyboard();
                _isOpen = true;
            }
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
    }

    public static bool Hide()
    {
        if (!_isOpen)
            return false;

        try
        {
            if (OperatingSystem.IsWindows())
                HideWindowsKeyboard();
            else if (OperatingSystem.IsLinux())
                OpenSteamUri(SteamCloseKeyboardUri);
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
        finally
        {
            _isOpen = false;
        }

        return true;
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

    private static void ShowSteamKeyboard()
    {
        OpenSteamUri(SteamKeyboardUri);
    }

    private static void OpenSteamUri(string uri)
    {
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
}
