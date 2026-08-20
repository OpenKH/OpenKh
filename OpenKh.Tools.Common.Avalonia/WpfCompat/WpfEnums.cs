// WPF compatibility shims for the Avalonia builds of OpenKh tools.
//
// These types are declared in the System.Windows namespace on purpose: the
// ModsManager ViewModels/Services are compiled into both the WPF and the
// Avalonia executables from the same source files, so keeping the WPF type
// names resolvable lets the shared sources build unchanged. Only the Avalonia
// projects reference this assembly; the WPF projects keep using the real WPF
// types, so the names never collide.

namespace System.Windows
{
    public enum MessageBoxButton
    {
        OK = 0,
        OKCancel = 1,
        YesNoCancel = 3,
        YesNo = 4,
    }

    public enum MessageBoxImage
    {
        None = 0,
        Error = 16,
        Hand = 16,
        Stop = 16,
        Question = 32,
        Exclamation = 48,
        Warning = 48,
        Asterisk = 64,
        Information = 64,
    }

    public enum MessageBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 6,
        No = 7,
    }

    [Flags]
    public enum MessageBoxOptions
    {
        None = 0,
        ServiceNotification = 0x00200000,
        DefaultDesktopOnly = 0x00020000,
        RightAlign = 0x00080000,
        RtlReading = 0x00100000,
    }

    public enum Visibility
    {
        Visible = 0,
        Hidden = 1,
        Collapsed = 2,
    }

    // Values intentionally line up with Avalonia.Controls.WindowStartupLocation
    // so DialogWindowBase can cast between them directly.
    public enum WindowStartupLocation
    {
        Manual = 0,
        CenterScreen = 1,
        CenterOwner = 2,
    }
}
