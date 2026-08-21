using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenKh.Tools.Launcher;

internal static class DesktopShortcutService
{
    public static string CreateModManagerShortcut(
        string targetPath,
        string? shortcutDirectory = null) =>
        CreateModManagerShortcut(targetPath, null, shortcutDirectory);

    public static string CreateModManagerShortcut(
        string targetPath,
        IEnumerable<string>? arguments,
        string? shortcutDirectory = null)
    {
        if (!OperatingSystem.IsWindows())
            return CreateLinuxShortcut(targetPath, arguments, shortcutDirectory);

        shortcutDirectory ??= Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var shortcutPath = Path.Combine(shortcutDirectory, "OpenKH Mod Manager.lnk");
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Windows Script Host is not available.");
        object? shell = null;
        object? shortcut = null;

        try
        {
            shell = Activator.CreateInstance(shellType)
                ?? throw new InvalidOperationException("Windows Script Host could not be started.");
            shortcut = shellType.InvokeMember(
                "CreateShortcut",
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                shell,
                new object[] { shortcutPath }
            ) ?? throw new InvalidOperationException("The shortcut could not be created.");

            var shortcutType = shortcut.GetType();
            shortcutType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
            var argumentText = string.Join(" ", arguments ?? []);
            if (argumentText.Length > 0)
                shortcutType.InvokeMember("Arguments", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { argumentText });
            shortcutType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath)! });
            shortcutType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "Open OpenKH Mod Manager" });
            shortcutType.InvokeMember("IconLocation", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { $"{targetPath},0" });
            shortcutType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
        }
        finally
        {
            if (shortcut != null && Marshal.IsComObject(shortcut))
                Marshal.FinalReleaseComObject(shortcut);
            if (shell != null && Marshal.IsComObject(shell))
                Marshal.FinalReleaseComObject(shell);
        }

        return shortcutPath;
    }

    private static string CreateLinuxShortcut(
        string targetPath,
        IEnumerable<string>? arguments,
        string? shortcutDirectory)
    {
        if (string.IsNullOrWhiteSpace(shortcutDirectory))
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            shortcutDirectory = Directory.Exists(desktop)
                ? desktop
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local",
                    "share",
                    "applications"
                );
        }

        Directory.CreateDirectory(shortcutDirectory);
        var shortcutPath = Path.Combine(shortcutDirectory, "openkh-mod-manager.desktop");
        var escapedTarget = EscapeDesktopEntryValue(targetPath);
        var escapedArguments = string.Join(" ", (arguments ?? []).Select(EscapeDesktopEntryArgument));
        var exec = escapedArguments.Length == 0
            ? $"\"{escapedTarget}\""
            : $"\"{escapedTarget}\" {escapedArguments}";
        var content = new StringBuilder()
            .AppendLine("[Desktop Entry]")
            .AppendLine("Type=Application")
            .AppendLine("Name=OpenKH Mod Manager")
            .AppendLine("Comment=Open OpenKH Mod Manager")
            .AppendLine($"Exec={exec}")
            .AppendLine($"Path=\"{EscapeDesktopEntryValue(Path.GetDirectoryName(targetPath) ?? string.Empty)}\"")
            .AppendLine("Terminal=false")
            .AppendLine("Categories=Game;Utility;")
            .ToString();
        File.WriteAllText(shortcutPath, content, new UTF8Encoding(false));
        if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            File.SetUnixFileMode(
                shortcutPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute
            );
        }
        return shortcutPath;
    }

    private static string EscapeDesktopEntryValue(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string EscapeDesktopEntryArgument(string value) =>
        $"\"{EscapeDesktopEntryValue(value)}\"";
}
