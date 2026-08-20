using OpenKh.Tools.ModsManager.Services;
using OpenKh.Patcher;
using Xunit;

namespace OpenKh.Tests.ModsManager.Avalonia;

public class LinuxPlatformTests
{
    [Fact]
    public void WindowsOnlyCapabilitiesAreDisabled()
    {
        if (!OperatingSystem.IsLinux())
            return;

        Assert.False(PlatformCapabilities.SupportsPcsx2Injection);
        Assert.False(PlatformCapabilities.SupportsPanacea);
        Assert.False(PlatformCapabilities.SupportsEpicGamesStore);
        Assert.False(PlatformCapabilities.SupportsSelfUpdate);
    }

    [Theory]
    [InlineData("/home/openkh/mods", "Z:\\home\\openkh\\mods")]
    [InlineData("/tmp/openkh", "Z:\\tmp\\openkh")]
    public void GamePathsUseWineZDriveOnLinux(string linuxPath, string expected)
    {
        if (!OperatingSystem.IsLinux())
            return;

        Assert.Equal(expected, WinePathUtil.ToGamePath(linuxPath));
    }

    [Fact]
    public void ForwardSlashGamePathsAreTomlFriendlyOnLinux()
    {
        if (!OperatingSystem.IsLinux())
            return;

        Assert.Equal("Z:/home/openkh/mods", WinePathUtil.ToGamePathForwardSlashes("/home/openkh/mods"));
    }

    [Fact]
    public void WindowsStyleModAssetPathsAreNormalizedOnLinux()
    {
        if (!OperatingSystem.IsLinux())
            return;

        Assert.Equal("bgm/music050.win32.scd", PatcherProcessor.Context.NormalizeSeparators("bgm\\music050.win32.scd"));
    }

    [Fact]
    public void ContextPathHelpersResolveWindowsStyleAssetNamesOnLinux()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var context = new PatcherProcessor.Context(
            metadata: null!,
            originalAssetPath: "/original",
            sourceModAssetPath: "/source",
            destinationPath: "/destination");

        Assert.Equal("/original/bgm/music050.win32.scd", context.GetOriginalAssetPath("bgm\\music050.win32.scd"));
        Assert.Equal("/source/bgm/music050.win32.scd", context.GetSourceModAssetPath("bgm\\music050.win32.scd"));
        Assert.Equal("/destination/bgm/music050.win32.scd", context.GetDestinationPath("bgm\\music050.win32.scd"));
    }
}
