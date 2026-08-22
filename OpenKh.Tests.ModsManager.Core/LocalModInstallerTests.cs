using OpenKh.Tools.ModsManager.Core;
using System.IO.Compression;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public sealed class LocalModInstallerTests : IDisposable
{
    private readonly string _rootDirectory = Path.Combine(
        Path.GetTempPath(),
        "OpenKhLocalInstallerTests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task InstallExtractsWrappedPackageIntoGameDirectory()
    {
        var packagePath = CreatePackage(
            "Example.zip",
            ("Example-main/mod.yml", "title: Example Mod\noriginalAuthor: Tester\nassets: []"),
            ("Example-main/files/content.bin", "content"));
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var installer = new LocalModInstaller(layout);

        var result = await installer.InstallAsync(packagePath, GameInfo.FromId("kh2"));

        Assert.Equal("Example Mod", result.DisplayName);
        Assert.True(File.Exists(Path.Combine(result.Directory, "mod.yml")));
        Assert.True(File.Exists(Path.Combine(result.Directory, "files", "content.bin")));
    }

    [Fact]
    public async Task InstallRejectsEntriesOutsideDestination()
    {
        var packagePath = CreatePackage(
            "Unsafe.zip",
            ("mod.yml", "title: Unsafe\nassets: []"),
            ("../outside.txt", "unsafe"));
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var installer = new LocalModInstaller(layout);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            installer.InstallAsync(packagePath, GameInfo.FromId("kh2")));

        Assert.False(File.Exists(Path.Combine(_rootDirectory, "mods", "kh2", "outside.txt")));
    }

    [Fact]
    public async Task InstallRejectsPatchEntriesOutsideDestination()
    {
        var packagePath = CreatePackage(
            "Unsafe.kh2pcpatch",
            ("package/original/../../outside.txt", "unsafe"));
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var installer = new LocalModInstaller(layout);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            installer.InstallAsync(packagePath, GameInfo.FromId("kh2")));

        Assert.False(File.Exists(Path.Combine(_rootDirectory, "mods", "kh2", "outside.txt")));
    }

    [Fact]
    public async Task InstallRequestsConfirmationWhenModAlreadyExists()
    {
        var packagePath = CreatePackage(
            "Example.zip",
            ("mod.yml", "title: Example Mod\nassets: []"));
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var installer = new LocalModInstaller(layout);
        await installer.InstallAsync(packagePath, GameInfo.FromId("kh2"));

        var exception = await Assert.ThrowsAsync<ModAlreadyInstalledException>(() =>
            installer.InstallAsync(packagePath, GameInfo.FromId("kh2")));

        Assert.Equal("Example", exception.ModName);
    }

    [Fact]
    public async Task FindInstalledModDetectsAnExistingPackageWithoutThrowing()
    {
        var packagePath = CreatePackage(
            "Example.zip",
            ("mod.yml", "title: Example Mod\nassets: []"));
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var installer = new LocalModInstaller(layout);

        Assert.Null(installer.FindInstalledMod(packagePath, GameInfo.FromId("kh2")));
        await installer.InstallAsync(packagePath, GameInfo.FromId("kh2"));

        Assert.Equal("Example", installer.FindInstalledMod(packagePath, GameInfo.FromId("kh2")));
    }

    [Fact]
    public async Task FindInstalledModDetectsAnExistingPcPatchWithoutThrowing()
    {
        var packagePath = CreatePackage(
            "Example.kh2pcpatch",
            ("package/original/obj/example.bin", "content"));
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var installer = new LocalModInstaller(layout);

        Assert.Null(installer.FindInstalledMod(packagePath, GameInfo.FromId("kh2")));
        await installer.InstallAsync(packagePath, GameInfo.FromId("kh2"));

        Assert.Equal("Example", installer.FindInstalledMod(packagePath, GameInfo.FromId("kh2")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
            Directory.Delete(_rootDirectory, true);
    }

    private string CreatePackage(string name, params (string Name, string Content)[] entries)
    {
        Directory.CreateDirectory(_rootDirectory);
        var packagePath = Path.Combine(_rootDirectory, name);
        using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
        foreach (var item in entries)
        {
            var entry = archive.CreateEntry(item.Name);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(item.Content);
        }

        return packagePath;
    }
}
