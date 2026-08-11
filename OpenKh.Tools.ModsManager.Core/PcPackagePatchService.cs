using OpenKh.Common;
using OpenKh.Egs;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class PcPackagePatchService(ModManagerConfigurationService configuration)
{
    private const string OriginalFilesDirectory = "original";
    private const string RawFilesDirectory = "raw";

    public Task ApplyAsync(
        GameInfo game,
        bool fastMode = false,
        IProgress<ModOperationProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Apply(game, fastMode, progress, cancellationToken), cancellationToken);

    public Task RestoreAsync(
        GameInfo game,
        bool restorePackages = true,
        IProgress<ModOperationProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Restore(game, restorePackages, progress, cancellationToken), cancellationToken);

    private void Apply(
        GameInfo game,
        bool fastMode,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (configuration.Current.GameEdition != 2)
            throw new InvalidOperationException("Direct package patching is only available for PC releases.");

        var modOutput = configuration.GetGameModOutputDirectory(game);
        var packageMapFile = Path.Combine(modOutput, "patch-package-map.txt");
        if (!File.Exists(packageMapFile))
            throw new FileNotFoundException("Build the enabled mods before applying them to the game.", packageMapFile);

        var packageMap = File.ReadLines(packageMapFile)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(" $$$$ ", 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);
        StagePackages(modOutput, packageMap, cancellationToken);

        var packageDirectories = Directory.EnumerateDirectories(modOutput)
            .Where(directory => !Path.GetFileName(directory).StartsWith(".", StringComparison.Ordinal))
            .ToArray();
        for (var index = 0; index < packageDirectories.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = packageDirectories[index];
            var packagePart = Path.GetFileName(directory);
            progress?.Report(new ModOperationProgress(
                $"Applying {packagePart}",
                (double)index / Math.Max(1, packageDirectories.Length)));
            PatchPackage(game, directory, packagePart, fastMode, cancellationToken);
        }

        progress?.Report(new ModOperationProgress("Game packages were patched", 1));
    }

    private static void StagePackages(
        string modOutput,
        IReadOnlyDictionary<string, string> packageMap,
        CancellationToken cancellationToken)
    {
        var stagingDirectory = Path.Combine(modOutput, ".patch-staging");
        DeleteDirectoryIfPresent(stagingDirectory);
        Directory.CreateDirectory(stagingDirectory);
        foreach (var entry in packageMap)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = Path.Combine(modOutput, entry.Key);
            var destination = Path.Combine(stagingDirectory, entry.Value);
            if (!File.Exists(source))
                continue;
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Move(source, destination, true);
        }

        foreach (var directory in Directory.EnumerateDirectories(modOutput).Where(path => path != stagingDirectory))
            DeleteDirectoryIfPresent(directory);
        foreach (var directory in Directory.EnumerateDirectories(stagingDirectory))
            Directory.Move(directory, Path.Combine(modOutput, Path.GetFileName(directory)));
        DeleteDirectoryIfPresent(stagingDirectory);

        var specialDirectory = Path.Combine(modOutput, "special");
        if (!Directory.Exists(specialDirectory))
            return;
        foreach (var directory in Directory.EnumerateDirectories(specialDirectory))
        {
            var destination = Path.Combine(modOutput, Path.GetFileName(directory));
            if (Directory.Exists(destination))
                MergeDirectory(directory, destination);
            else
                Directory.Move(directory, destination);
        }
        DeleteDirectoryIfPresent(specialDirectory);
    }

    private void PatchPackage(
        GameInfo game,
        string patchDirectory,
        string packagePart,
        bool fastMode,
        CancellationToken cancellationToken)
    {
        var patchFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var originalDirectory = Path.Combine(patchDirectory, OriginalFilesDirectory);
        var rawDirectory = Path.Combine(patchDirectory, RawFilesDirectory);
        if (Directory.Exists(originalDirectory))
            patchFiles.UnionWith(OpenKh.Egs.Helpers.GetAllFiles(originalDirectory));
        if (Directory.Exists(rawDirectory))
            patchFiles.UnionWith(OpenKh.Egs.Helpers.GetAllFiles(rawDirectory));

        var packageName = GetPackageName(game, packagePart, fastMode);
        var releaseDirectory = GetReleaseDirectory(game);
        var languageDirectory = configuration.Current.PcVersion.Equals("Steam", StringComparison.OrdinalIgnoreCase) &&
                                configuration.Current.PcReleaseLanguage.Equals("en", StringComparison.OrdinalIgnoreCase)
            ? "dt"
            : configuration.Current.PcReleaseLanguage;
        var gamePackage = Path.Combine(releaseDirectory, "Image", languageDirectory, $"{packageName}.pkg");
        var gameHed = Path.ChangeExtension(gamePackage, "hed");
        if (!File.Exists(gamePackage) || !File.Exists(gameHed))
            throw new FileNotFoundException($"Could not find the {packageName} HED/PKG files.", gamePackage);

        var backupDirectory = Path.Combine(releaseDirectory, "BackupImage");
        Directory.CreateDirectory(backupDirectory);
        var backupPackage = Path.Combine(backupDirectory, $"{packageName}.pkg");
        var backupHed = Path.ChangeExtension(backupPackage, "hed");
        if (!File.Exists(backupPackage))
        {
            File.Copy(gamePackage, backupPackage);
            File.Copy(gameHed, backupHed);
        }
        else
        {
            File.Copy(backupPackage, gamePackage, true);
            File.Copy(backupHed, gameHed, true);
        }

        var temporaryDirectory = Path.Combine(configuration.InstallationDirectory, ".openkh-patched-packages");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPackage = Path.Combine(temporaryDirectory, $"{packageName}-{Guid.NewGuid():N}.pkg");
        var temporaryHed = Path.ChangeExtension(temporaryPackage, "hed");
        try
        {
            using var hedStream = File.OpenRead(gameHed);
            using var packageStream = File.OpenRead(gamePackage);
            using var patchedHed = File.Create(temporaryHed);
            using var patchedPackage = File.Create(temporaryPackage);
            foreach (var header in Hed.Read(hedStream))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var hash = OpenKh.Egs.Helpers.ToString(header.MD5);
                if (!EgsTools.Names.TryGetValue(hash, out var fileName))
                    continue;

                var asset = new EgsHdAsset(packageStream.SetPosition(header.Offset));
                EgsTools.ReplaceFile(patchDirectory, fileName, patchedHed, patchedPackage, asset, header);
                patchFiles.Remove(fileName);
            }
            foreach (var fileName in patchFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                EgsTools.AddFile(patchDirectory, fileName, patchedHed, patchedPackage);
            }
        }
        catch
        {
            File.Copy(backupPackage, gamePackage, true);
            File.Copy(backupHed, gameHed, true);
            throw;
        }

        File.Move(temporaryPackage, gamePackage, true);
        File.Move(temporaryHed, gameHed, true);
    }

    private void Restore(
        GameInfo game,
        bool restorePackages,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (!restorePackages)
        {
            DeleteDirectoryIfPresent(configuration.GetGameModOutputDirectory(game));
            progress?.Report(new ModOperationProgress("Fast restore completed", 1));
            return;
        }

        var releaseDirectory = GetReleaseDirectory(game);
        var backupDirectory = Path.Combine(releaseDirectory, "BackupImage");
        if (!Directory.Exists(backupDirectory))
            throw new DirectoryNotFoundException("No BackupImage folder exists for this game installation.");

        var languageDirectory = configuration.Current.PcVersion.Equals("Steam", StringComparison.OrdinalIgnoreCase) &&
                                configuration.Current.PcReleaseLanguage.Equals("en", StringComparison.OrdinalIgnoreCase)
            ? "dt"
            : configuration.Current.PcReleaseLanguage;
        var imageDirectory = Path.Combine(releaseDirectory, "Image", languageDirectory);
        var packages = Directory.EnumerateFiles(backupDirectory, "*.pkg")
            .Where(file => Path.GetFileName(file).Contains(game.Id, StringComparison.OrdinalIgnoreCase) ||
                           game.Id.Equals("Recom", StringComparison.OrdinalIgnoreCase) &&
                           Path.GetFileName(file).Contains("Recom", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (packages.Length == 0)
            throw new FileNotFoundException($"No backup packages were found for {game.DisplayName}.");

        for (var index = 0; index < packages.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var backupPackage = packages[index];
            var destinationPackage = Path.Combine(imageDirectory, Path.GetFileName(backupPackage));
            File.Copy(backupPackage, destinationPackage, true);
            File.Copy(Path.ChangeExtension(backupPackage, "hed"), Path.ChangeExtension(destinationPackage, "hed"), true);
            progress?.Report(new ModOperationProgress(
                $"Restoring {Path.GetFileNameWithoutExtension(backupPackage)}",
                (double)(index + 1) / packages.Length));
        }
        DeleteDirectoryIfPresent(configuration.GetGameModOutputDirectory(game));
    }

    private string GetReleaseDirectory(GameInfo game)
    {
        var directory = game.Id.Equals("kh3d", StringComparison.OrdinalIgnoreCase)
            ? configuration.Current.PcReleaseLocationKh3D
            : configuration.Current.PcReleaseLocation;
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Configure the {game.DisplayName} installation folder in Setup.");
        return directory;
    }

    private static string GetPackageName(GameInfo game, string packagePart, bool fastMode) =>
        game.Id.ToLowerInvariant() switch
        {
            "kh1" => fastMode ? "kh1_first" : packagePart,
            "bbs" => fastMode ? "bbs_first" : packagePart,
            "recom" => "Recom",
            "kh3d" => fastMode ? "kh3d_first" : packagePart,
            _ => fastMode ? "kh2_first" : packagePart
        };

    private static void MergeDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.EnumerateDirectories(source))
            MergeDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
        foreach (var file in Directory.EnumerateFiles(source))
            File.Move(file, Path.Combine(destination, Path.GetFileName(file)), true);
        DeleteDirectoryIfPresent(source);
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
