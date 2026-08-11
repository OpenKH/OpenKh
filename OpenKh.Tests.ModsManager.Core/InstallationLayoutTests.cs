using OpenKh.Tools.ModsManager.Core;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public sealed class InstallationLayoutTests
{
    [Fact]
    public void DetectUsesParentOfAppsDirectory()
    {
        var installationDirectory = Path.Combine(Path.GetTempPath(), "OpenKhLayoutTest");
        var layout = InstallationLayout.Detect(Path.Combine(installationDirectory, "Apps"));

        Assert.Equal(Path.GetFullPath(installationDirectory), layout.RootDirectory);
    }

    [Fact]
    public void DetectUsesLegacyModManagerInstallationRoot()
    {
        var installationDirectory = Path.Combine(Path.GetTempPath(), "OpenKhLegacyLayoutTest");
        var layout = InstallationLayout.Detect(Path.Combine(installationDirectory, "Apps", "ModManager"));

        Assert.Equal(Path.GetFullPath(installationDirectory), layout.RootDirectory);
    }

    [Fact]
    public void DetectPrefersDataRootArgument()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "OpenKhCustomData");
        var layout = InstallationLayout.Detect("ignored", ["--data-root", dataRoot]);

        Assert.Equal(Path.GetFullPath(dataRoot), layout.RootDirectory);
    }
}
