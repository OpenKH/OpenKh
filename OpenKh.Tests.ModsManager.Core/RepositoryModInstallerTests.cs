using OpenKh.Tools.ModsManager.Core;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public sealed class RepositoryModInstallerTests
{
    [Fact]
    public void ParseSupportsRepositoryHostShorthand()
    {
        var address = RepositoryModInstaller.RepositoryAddress.Parse(
            "TopazTK/KH2-ArchipelagoEnablers@codeberg.org",
            null);

        Assert.Equal("TopazTK/KH2-ArchipelagoEnablers", address.Id);
        Assert.Equal(
            "https://codeberg.org/TopazTK/KH2-ArchipelagoEnablers.git",
            address.CloneUrl);
        Assert.Null(address.Branch);
    }

    [Fact]
    public void ParseKeepsBranchWithRepositoryHostShorthand()
    {
        var address = RepositoryModInstaller.RepositoryAddress.Parse(
            "owner/repository/feature/testing@gitlab.com",
            null);

        Assert.Equal("https://gitlab.com/owner/repository.git", address.CloneUrl);
        Assert.Equal("feature/testing", address.Branch);
    }

    [Fact]
    public void ParseSupportsCompleteNonGithubUrl()
    {
        var address = RepositoryModInstaller.RepositoryAddress.Parse(
            "https://codeberg.org/owner/repository",
            "release");

        Assert.Equal("owner/repository", address.Id);
        Assert.Equal("https://codeberg.org/owner/repository.git", address.CloneUrl);
        Assert.Equal("release", address.Branch);
    }
}
