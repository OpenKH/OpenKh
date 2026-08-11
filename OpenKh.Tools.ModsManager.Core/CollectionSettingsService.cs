using OpenKh.Patcher;
using System.Text.Json;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class CollectionSettingsService(ModManagerConfigurationService configuration)
{
    public IReadOnlyList<CollectionOption> GetOptions(ModEntry mod, GameInfo game)
    {
        if (!mod.IsCollection)
            return [];

        using var stream = File.OpenRead(Path.Combine(mod.Directory, "mod.yml"));
        var metadata = Metadata.Read(stream);
        var settings = ReadSettings(game);
        settings.TryGetValue(mod.Id, out var modSettings);
        return (metadata.Assets ?? [])
            .Where(asset => asset.CollectionOptional)
            .Select(asset => new CollectionOption(
                asset.Name,
                modSettings?.TryGetValue(asset.Name, out var enabled) == true && enabled))
            .ToArray();
    }

    public void SaveOptions(ModEntry mod, GameInfo game, IEnumerable<CollectionOption> options)
    {
        var settings = ReadSettings(game);
        settings[mod.Id] = options.ToDictionary(
            option => option.Name,
            option => option.IsEnabled,
            StringComparer.OrdinalIgnoreCase);
        var fileName = configuration.GetCollectionSettingsFile(game);
        File.WriteAllText(fileName, JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private Dictionary<string, Dictionary<string, bool>> ReadSettings(GameInfo game)
    {
        var fileName = configuration.GetCollectionSettingsFile(game);
        if (!File.Exists(fileName) || string.IsNullOrWhiteSpace(File.ReadAllText(fileName)))
            return new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(File.ReadAllText(fileName))
            ?? new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);
    }
}

public sealed record CollectionOption(string Name, bool IsEnabled);
