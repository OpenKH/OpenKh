using OpenKh.Patcher;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class ModBuildService(ModManagerConfigurationService configuration)
{
    public Task<string> BuildAsync(
        GameInfo game,
        IReadOnlyList<ModEntry> mods,
        bool fastMode = false,
        IProgress<ModOperationProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Build(game, mods, fastMode, progress, cancellationToken), cancellationToken);

    private string Build(
        GameInfo game,
        IReadOnlyList<ModEntry> mods,
        bool fastMode,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var outputDirectory = configuration.GetGameModOutputDirectory(game);
        DeleteDirectoryIfPresent(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var enabledMods = mods.Where(mod => mod.IsEnabled).Reverse().ToArray();
        var collectionSettings = ReadCollectionSettings(game);
        var packageMap = new ConcurrentDictionary<string, string>();
        var patcher = new PatcherProcessor();

        for (var index = 0; index < enabledMods.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mod = enabledMods[index];
            progress?.Report(new ModOperationProgress(
                $"Building {mod.Name}",
                enabledMods.Length == 0 ? 1 : (double)index / enabledMods.Length));
            var metadataFile = Path.Combine(mod.Directory, "mod.yml");
            using var stream = File.OpenRead(metadataFile);
            var metadata = Metadata.Read(stream);
            collectionSettings.TryGetValue(mod.Id, out var optionalAssets);
            var progressLock = new object();
            var lastReportedPercentage = -1;
            void ReportPatchProgress(int completed, int total)
            {
                var modPercentage = total == 0 ? 100 : completed * 100 / total;
                lock (progressLock)
                {
                    if (modPercentage <= lastReportedPercentage)
                        return;

                    lastReportedPercentage = modPercentage;
                    var overallPercentage = (index + modPercentage / 100d) / enabledMods.Length;
                    progress?.Report(new ModOperationProgress(
                        $"Building {mod.Name}",
                        overallPercentage));
                }
            }
            patcher.Patch(
                Path.Combine(configuration.GameDataDirectory, game.Id),
                outputDirectory,
                metadata,
                mod.Directory,
                configuration.Current.GameEdition,
                fastMode,
                packageMap,
                game.Id,
                configuration.Current.PcReleaseLanguage,
                false,
                optionalAssets,
                ReportPatchProgress);
        }

        using var writer = File.CreateText(Path.Combine(outputDirectory, "patch-package-map.txt"));
        foreach (var entry in packageMap.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            writer.WriteLine($"{entry.Key} $$$$ {entry.Value}");

        progress?.Report(new ModOperationProgress("Mod build completed", 1));
        return outputDirectory;
    }

    private Dictionary<string, Dictionary<string, bool>> ReadCollectionSettings(GameInfo game)
    {
        var fileName = configuration.GetCollectionSettingsFile(game);
        if (!File.Exists(fileName) || string.IsNullOrWhiteSpace(File.ReadAllText(fileName)))
            return new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);

        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(File.ReadAllText(fileName))
            ?? new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);
    }

    private static void DeleteDirectoryIfPresent(string directory)
    {
        if (!Directory.Exists(directory))
            return;
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(directory, true);
    }
}
