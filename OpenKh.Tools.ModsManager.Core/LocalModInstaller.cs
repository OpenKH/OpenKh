using System.IO.Compression;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class LocalModInstaller
{
    private const string MetadataFileName = "mod.yml";
    private readonly InstallationLayout _layout;
    private readonly ModManagerConfiguration _configuration;

    public LocalModInstaller(InstallationLayout layout)
    {
        _layout = layout;
        _configuration = ModManagerConfiguration.Load(layout.ConfigurationFile);
    }

    public Task<ModInstallResult> InstallAsync(string packagePath, GameInfo game) =>
        Task.Run(() => Install(packagePath, game));

    private ModInstallResult Install(string packagePath, GameInfo game)
    {
        if (!File.Exists(packagePath))
            throw new FileNotFoundException("The selected mod package does not exist.", packagePath);

        using var archive = ZipFile.OpenRead(packagePath);
        var packageLayout = GetPackageLayout(archive);
        var metadataEntry = archive.Entries.First(entry =>
            NormalizeEntryName(entry.FullName).Equals(
                $"{packageLayout.Prefix}{MetadataFileName}",
                StringComparison.OrdinalIgnoreCase));

        ModMetadata metadata;
        using (var metadataReader = new StreamReader(metadataEntry.Open()))
            metadata = ModMetadata.Read(metadataReader);

        var packageName = CreateSafeDirectoryName(Path.GetFileNameWithoutExtension(packagePath));
        var destinationRoot = metadata.IsCollection
            ? GetCollectionsDirectory()
            : GetGameModsDirectory(game);
        var destinationDirectory = Path.Combine(destinationRoot, packageName);

        if (Directory.Exists(destinationDirectory))
        {
            throw new IOException(
                $"A mod named '{packageName}' is already installed. Remove it before installing this package again.");
        }

        Directory.CreateDirectory(destinationDirectory);
        try
        {
            foreach (var entry in archive.Entries)
                ExtractEntry(entry, packageLayout.Prefix, destinationDirectory);

            if (!File.Exists(Path.Combine(destinationDirectory, MetadataFileName)))
                throw new InvalidDataException("The package did not extract a mod.yml file.");
        }
        catch
        {
            Directory.Delete(destinationDirectory, true);
            throw;
        }

        return new ModInstallResult(
            packageName,
            string.IsNullOrWhiteSpace(metadata.Title) ? packageName : metadata.Title,
            destinationDirectory);
    }

    private static PackageLayout GetPackageLayout(ZipArchive archive)
    {
        var fileNames = archive.Entries
            .Where(entry => !string.IsNullOrEmpty(entry.Name))
            .Select(entry => NormalizeEntryName(entry.FullName))
            .ToArray();

        if (fileNames.Any(name => name.Equals(MetadataFileName, StringComparison.OrdinalIgnoreCase)))
            return new PackageLayout(string.Empty);

        var metadataEntry = fileNames
            .Where(name => name.EndsWith($"/{MetadataFileName}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name.Count(character => character == '/'))
            .FirstOrDefault();

        if (metadataEntry is null)
            throw new InvalidDataException("This package is not a valid OpenKH mod because mod.yml is missing.");

        var prefix = metadataEntry[..^MetadataFileName.Length];
        if (fileNames.Any(name => !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException(
                "The package contains files outside the directory that contains mod.yml.");
        }

        return new PackageLayout(prefix);
    }

    private static void ExtractEntry(ZipArchiveEntry entry, string prefix, string destinationDirectory)
    {
        var entryName = NormalizeEntryName(entry.FullName);
        if (!entryName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return;

        var relativeName = entryName[prefix.Length..];
        if (string.IsNullOrWhiteSpace(relativeName) || string.IsNullOrEmpty(entry.Name))
            return;

        var destinationPath = Path.GetFullPath(Path.Combine(
            destinationDirectory,
            relativeName.Replace('/', Path.DirectorySeparatorChar)));
        var relativeDestination = Path.GetRelativePath(destinationDirectory, destinationPath);
        if (relativeDestination.Equals("..", StringComparison.Ordinal) ||
            relativeDestination.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new InvalidDataException("The package contains a file outside its destination directory.");
        }

        var parentDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(parentDirectory))
            Directory.CreateDirectory(parentDirectory);
        entry.ExtractToFile(destinationPath, false);
    }

    private string GetGameModsDirectory(GameInfo game)
    {
        var collectionRoot = ResolveConfiguredPath(
            _configuration.ModCollectionPath,
            _layout.RootDirectory);
        return Path.Combine(collectionRoot, "mods", game.Id);
    }

    private string GetCollectionsDirectory() => ResolveConfiguredPath(
        _configuration.ModCollectionsPath,
        Path.Combine(_layout.RootDirectory, "mods", "collections"));

    private string ResolveConfiguredPath(string? configuredPath, string fallbackPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            return fallbackPath;

        return Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_layout.RootDirectory, configuredPath));
    }

    private static string NormalizeEntryName(string name) => name.Replace('\\', '/').TrimStart('/');

    private static string CreateSafeDirectoryName(string name)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();
        var safeName = new string(name.Select(character =>
            invalidCharacters.Contains(character) ? '_' : character).ToArray()).Trim();
        return string.IsNullOrEmpty(safeName) ? "InstalledMod" : safeName;
    }

    private sealed record PackageLayout(string Prefix);
}

public sealed record ModInstallResult(string Id, string DisplayName, string Directory);
