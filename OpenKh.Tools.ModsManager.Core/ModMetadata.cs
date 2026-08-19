using OpenKh.Patcher;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Tools.ModsManager.Core;

internal sealed class ModMetadata
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .IgnoreFields()
        .IgnoreUnmatchedProperties()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public string? Title { get; set; }
    public string? OriginalAuthor { get; set; }
    public string? Description { get; set; }
    public bool IsCollection { get; set; }
    public List<AssetFile>? Assets { get; set; }

    public static ModMetadata Read(string fileName)
    {
        using var reader = File.OpenText(fileName);
        return Deserializer.Deserialize<ModMetadata>(reader) ?? new ModMetadata();
    }

    public static ModMetadata Read(TextReader reader) =>
        Deserializer.Deserialize<ModMetadata>(reader) ?? new ModMetadata();
}
