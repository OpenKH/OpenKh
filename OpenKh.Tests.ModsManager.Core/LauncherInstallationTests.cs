using OpenKh.Tools.Launcher.Updates;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public sealed class LauncherInstallationTests
{
    [Fact]
    public void DetectRootSupportsCurrentAndPreviousLayouts()
    {
        var installationDirectory = Path.Combine(Path.GetTempPath(), "OpenKhLauncherLayoutTest");

        Assert.Equal(
            Path.GetFullPath(installationDirectory),
            LauncherInstallation.DetectRoot(Path.Combine(installationDirectory, "Apps")));
        Assert.Equal(
            Path.GetFullPath(installationDirectory),
            LauncherInstallation.DetectRoot(Path.Combine(installationDirectory, "Apps", "ModManager")));
    }

    [Fact]
    public void FindModManagerExecutablePrefersCurrentLayout()
    {
        var installationDirectory = Path.Combine(
            Path.GetTempPath(),
            $"openkh-launcher-installation-{Guid.NewGuid():N}");
        var executableName = OperatingSystem.IsWindows()
            ? "OpenKh.Tools.ModsManager.exe"
            : "OpenKh.Tools.ModsManager";
        var rootExecutable = Path.Combine(installationDirectory, executableName);
        var previousExecutable = Path.Combine(
            installationDirectory,
            "Apps",
            "ModManager",
            executableName);
        var currentExecutable = Path.Combine(
            installationDirectory,
            "Apps",
            executableName);

        try
        {
            Directory.CreateDirectory(installationDirectory);
            File.WriteAllText(rootExecutable, string.Empty);
            Assert.Equal(
                rootExecutable,
                LauncherInstallation.FindModManagerExecutable(installationDirectory));

            Directory.CreateDirectory(Path.GetDirectoryName(previousExecutable)!);
            File.WriteAllText(previousExecutable, string.Empty);
            Assert.Equal(
                previousExecutable,
                LauncherInstallation.FindModManagerExecutable(installationDirectory));

            File.WriteAllText(currentExecutable, string.Empty);
            Assert.Equal(
                currentExecutable,
                LauncherInstallation.FindModManagerExecutable(installationDirectory));
        }
        finally
        {
            if (Directory.Exists(installationDirectory))
                Directory.Delete(installationDirectory, recursive: true);
        }
    }
}
