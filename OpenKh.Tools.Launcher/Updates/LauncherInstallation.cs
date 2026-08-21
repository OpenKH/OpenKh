namespace OpenKh.Tools.Launcher.Updates;

public static class LauncherInstallation
{
    public const string DataRootEnvironmentVariable = "OPENKH_DATA_ROOT";

    public static string RootDirectory => DetectRoot(AppContext.BaseDirectory);
    // AppImage mounts application files read-only, so AppRun redirects mutable data here.
    public static string DataDirectory => DetectDataDirectory(
        RootDirectory,
        Environment.GetEnvironmentVariable(DataRootEnvironmentVariable));
    public static string? AppImagePath => GetAbsoluteEnvironmentPath("APPIMAGE");
    public static bool IsAppImage => OperatingSystem.IsLinux() && AppImagePath is not null;

    public static string DetectRoot(string applicationBaseDirectory)
    {
        var applicationDirectory = new DirectoryInfo(
            Path.GetFullPath(applicationBaseDirectory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar));

        if (applicationDirectory.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase) &&
            applicationDirectory.Parent is not null)
        {
            return applicationDirectory.Parent.FullName;
        }

        if (applicationDirectory.Name.Equals("ModManager", StringComparison.OrdinalIgnoreCase) &&
            applicationDirectory.Parent?.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase) == true &&
            applicationDirectory.Parent.Parent is not null)
        {
            return applicationDirectory.Parent.Parent.FullName;
        }

        return applicationDirectory.FullName;
    }

    public static string FindModManagerExecutable(string installationDirectory)
    {
        var executableName = OperatingSystem.IsWindows()
            ? "OpenKh.Tools.ModsManager.exe"
            : "OpenKh.Tools.ModsManager";
        var candidates = new[]
        {
            Path.Combine(installationDirectory, "Apps", executableName),
            Path.Combine(installationDirectory, "Apps", "ModManager", executableName),
            Path.Combine(installationDirectory, executableName),
        };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    public static string DetectDataDirectory(string installationDirectory, string? configuredDataRoot) =>
        string.IsNullOrWhiteSpace(configuredDataRoot)
            ? Path.GetFullPath(installationDirectory)
            : Path.GetFullPath(configuredDataRoot);

    private static string? GetAbsoluteEnvironmentPath(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(value) ? null : Path.GetFullPath(value);
    }
}
