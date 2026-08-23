using System.Formats.Tar;
using System.IO.Compression;
using OpenKh.Tools.Launcher.Updates;
using Xunit;

namespace OpenKh.Tests.Launcher.Avalonia;

public sealed class UpdateArchiveTests : IDisposable
{
    private readonly string _rootDirectory = Path.Combine(
        Path.GetTempPath(),
        "OpenKhUpdateArchiveTests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void ExtractArchiveReadsPortableLinuxTarball()
    {
        var packageDirectory = Path.Combine(_rootDirectory, "source", "openkh-linux-x64");
        var applicationDirectory = Path.Combine(packageDirectory, "Apps");
        Directory.CreateDirectory(applicationDirectory);
        File.WriteAllText(Path.Combine(packageDirectory, "OpenKh.Launcher"), "launcher");
        File.WriteAllText(Path.Combine(applicationDirectory, "OpenKh.Tools.ModsManager"), "manager");
        var archivePath = Path.Combine(_rootDirectory, "openkh-linux-x64.tar.gz");
        Directory.CreateDirectory(_rootDirectory);
        using (var output = File.Create(archivePath))
        using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize))
            TarFile.CreateFromDirectory(packageDirectory, gzip, includeBaseDirectory: true);

        var extractionDirectory = Path.Combine(_rootDirectory, "extracted");
        Directory.CreateDirectory(extractionDirectory);
        OpenKhUpdateInstallerService.ExtractArchive(archivePath, extractionDirectory);

        Assert.Equal(
            "launcher",
            File.ReadAllText(Path.Combine(extractionDirectory, "openkh-linux-x64", "OpenKh.Launcher")));
        Assert.Equal(
            "manager",
            File.ReadAllText(Path.Combine(
                extractionDirectory,
                "openkh-linux-x64",
                "Apps",
                "OpenKh.Tools.ModsManager")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
            Directory.Delete(_rootDirectory, true);
    }
}
