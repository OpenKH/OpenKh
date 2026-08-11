using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class ModManagerConfiguration
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .IgnoreFields()
        .IgnoreUnmatchedProperties()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public string? ModCollectionPath { get; set; }
    public string? ModCollectionsPath { get; set; }
    public string LaunchGame { get; set; } = "kh2";

    public static ModManagerConfiguration Load(string fileName)
    {
        if (!File.Exists(fileName))
            return new ModManagerConfiguration();

        using var reader = File.OpenText(fileName);
        return Deserializer.Deserialize<ModManagerConfiguration>(reader) ?? new ModManagerConfiguration();
    }
}
