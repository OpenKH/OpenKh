namespace OpenKh.Tools.ModsManager.Core;

public static class OpenKhUpdateEnvironment
{
    public static string? AppImagePath => GetExistingAbsolutePath(
        Environment.GetEnvironmentVariable("APPIMAGE"));

    public static bool IsAppImage => OperatingSystem.IsLinux() && AppImagePath is not null;

    public static string ReleaseAssetName => OperatingSystem.IsWindows()
        ? "openkh.zip"
        : IsAppImage
            ? "openkh-x86_64.AppImage"
            : "openkh-linux-x64.tar.gz";

    public static string? FindLauncher(string installationDirectory, string applicationBaseDirectory)
    {
        if (AppImagePath is { } appImagePath)
            return appImagePath;

        var applicationRoot = FindApplicationRoot(applicationBaseDirectory);
        var executableName = OperatingSystem.IsWindows()
            ? "OpenKh.Launcher.exe"
            : "OpenKh.Launcher";
        var candidates = new[]
        {
            Path.Combine(installationDirectory, executableName),
            Path.Combine(applicationRoot, executableName),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    public static string FindVersionDirectory(
        string installationDirectory,
        string applicationBaseDirectory)
    {
        var applicationRoot = FindApplicationRoot(applicationBaseDirectory);
        return File.Exists(Path.Combine(applicationRoot, "openkh-release"))
            ? applicationRoot
            : installationDirectory;
    }

    public static string FindApplicationRoot(string applicationBaseDirectory)
    {
        var applicationDirectory = new DirectoryInfo(
            Path.GetFullPath(applicationBaseDirectory).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar));
        return applicationDirectory.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase) &&
               applicationDirectory.Parent is not null
            ? applicationDirectory.Parent.FullName
            : applicationDirectory.FullName;
    }

    private static string? GetExistingAbsolutePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var path = Path.GetFullPath(value);
        return File.Exists(path) ? path : null;
    }
}
