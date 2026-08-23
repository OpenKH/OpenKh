using OpenKh.Tools.ModsManager.Core;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public sealed class ModBuildServiceTests : IDisposable
{
    private readonly string _rootDirectory = Path.Combine(
        Path.GetTempPath(),
        "OpenKhModBuildServiceTests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task BuildReportsProgressWithinASingleMod()
    {
        var configuration = new ModManagerConfigurationService(
            InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]));
        configuration.EnsureDirectories();
        var modDirectory = Path.Combine(_rootDirectory, "test-mod");
        Directory.CreateDirectory(modDirectory);
        File.WriteAllText(Path.Combine(modDirectory, "source.bin"), "test data");
        File.WriteAllText(Path.Combine(modDirectory, "mod.yml"), """
            title: Progress test
            game: kh2
            assets:
            - name: first.bin
              multi:
              - name: second.bin
              - name: third.bin
              method: copy
              source:
              - name: source.bin
            """);
        var reports = new List<ModOperationProgress>();
        var progress = new RecordingProgress(reports);

        await new ModBuildService(configuration).BuildAsync(
            GameInfo.FromId("kh2"),
            [new ModEntry
            {
                Id = "progress-test",
                Name = "Progress test",
                Directory = modDirectory,
                IsEnabled = true
            }],
            progress: progress);

        var percentages = reports
            .Where(report => report.Percentage.HasValue)
            .Select(report => report.Percentage!.Value)
            .ToArray();
        Assert.Equal(0, percentages[0]);
        Assert.Contains(percentages, percentage => percentage > 0 && percentage < 1);
        Assert.Equal(1, percentages[^1]);
        Assert.True(percentages.SequenceEqual(percentages.OrderBy(value => value)));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
            Directory.Delete(_rootDirectory, true);
    }

    private sealed class RecordingProgress(List<ModOperationProgress> reports) : IProgress<ModOperationProgress>
    {
        public void Report(ModOperationProgress value) => reports.Add(value);
    }
}
