using OpenKh.Patcher;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class ModCreatorService(ModManagerConfigurationService? configuration = null)
{
    public string DefaultGameDataPath => configuration?.GameDataDirectory ?? string.Empty;
    public string Create(
        string directory,
        string title,
        string author,
        string description,
        GameInfo game)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Choose a mod folder.", nameof(directory));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Enter a mod title.", nameof(title));

        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "mod.yml");
        var metadata = ReadOrCreate(path);
        metadata.Title = title.Trim();
        metadata.OriginalAuthor = string.IsNullOrWhiteSpace(author) ? "Unknown author" : author.Trim();
        metadata.Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description.Trim();
        metadata.Game = game.Id;
        metadata.Assets ??= [];
        using var stream = File.Create(path);
        metadata.Write(stream);
        return path;
    }

    public Metadata ReadOrCreate(string modYmlPath)
    {
        if (!File.Exists(modYmlPath))
            return new Metadata { Assets = [] };
        using var stream = File.OpenRead(modYmlPath);
        var metadata = Metadata.Read(stream) ?? new Metadata();
        metadata.Assets ??= [];
        return metadata;
    }

    public string CreatePreview(
        string directory,
        string title,
        string author,
        string description,
        GameInfo game)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Choose a mod folder.", nameof(directory));
        var existingPath = Path.Combine(directory, "mod.yml");
        var metadata = ReadOrCreate(existingPath);
        metadata.Title = string.IsNullOrWhiteSpace(title) ? Path.GetFileName(directory) : title.Trim();
        metadata.OriginalAuthor = string.IsNullOrWhiteSpace(author) ? "Unknown author" : author.Trim();
        metadata.Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description.Trim();
        metadata.Game = game.Id;
        metadata.Assets ??= [];
        var previewPath = Path.Combine(Path.GetTempPath(), $"openkh-mod-preview-{Guid.NewGuid():N}.yml");
        using var stream = File.Create(previewPath);
        metadata.Write(stream);
        return previewPath;
    }

    public Task<IReadOnlyList<string>> SearchFilesAsync(
        string gameDataPath,
        string searchText,
        CancellationToken cancellationToken = default) => Task.Run<IReadOnlyList<string>>(() =>
    {
        if (string.IsNullOrWhiteSpace(gameDataPath) || !Directory.Exists(gameDataPath))
            throw new DirectoryNotFoundException("Choose a valid GameData folder.");
        var terms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return Directory.EnumerateFiles(gameDataPath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(gameDataPath, path).Replace('\\', '/'))
            .Where(path => terms.All(term => path.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }, cancellationToken);

    public void AppendCopyFiles(
        string modDirectory,
        string gameDataPath,
        IReadOnlyCollection<string> relativePaths)
    {
        var modYmlPath = Path.Combine(modDirectory, "mod.yml");
        if (!File.Exists(modYmlPath))
            throw new FileNotFoundException("Generate mod.yml before appending target files.", modYmlPath);
        var metadata = ReadOrCreate(modYmlPath);
        foreach (var relativePath in relativePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalized = relativePath.Replace('\\', '/');
            var sourcePath = Path.Combine(gameDataPath, normalized);
            if (!File.Exists(sourcePath))
                continue;
            var destinationPath = Path.Combine(modDirectory, normalized);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, true);
            metadata.Assets.RemoveAll(asset =>
                asset.Method == "copy" && string.Equals(asset.Name, normalized, StringComparison.OrdinalIgnoreCase));
            metadata.Assets.Add(new AssetFile
            {
                Name = normalized,
                Method = "copy",
                Source = [new AssetFile { Name = normalized }]
            });
        }
        using var output = File.Create(modYmlPath);
        metadata.Write(output);
    }

    public IReadOnlyList<CreatorPreference> GetPreferences() =>
        configuration?.Current.CreatorPreferences ?? [];

    public void SavePreference(CreatorPreference preference)
    {
        if (configuration is null)
            return;
        if (string.IsNullOrWhiteSpace(preference.Label))
            throw new ArgumentException("Enter a preference name.", nameof(preference));
        configuration.Current.CreatorPreferences.RemoveAll(item =>
            item.Label.Equals(preference.Label, StringComparison.OrdinalIgnoreCase));
        configuration.Current.CreatorPreferences.Add(preference);
        configuration.Save();
    }
}
