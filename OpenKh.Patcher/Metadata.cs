using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Patcher
{
    public class Metadata
    {
        public class Dependency
        {
            public string Name { get; set; }
        }

        public string Title { get; set; }
        public int Specifications { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public AssetContainer Assets { get; set; }

        private static readonly IDeserializer deserializer =
            new DeserializerBuilder()
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        private static readonly ISerializer serializer =
            new SerializerBuilder()
            .IgnoreFields()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public static Metadata Read(Stream stream) =>
            deserializer.Deserialize<Metadata>(new StreamReader(stream));
        public void Write(Stream stream) =>
            serializer.Serialize(new StreamWriter(stream), this);
    }

    public class AssetContainer
    {
        public AssetKh2 Kh2 { get; set; }
    }

    public class AssetKh2
    {
        public List<AssetBinary> Binaries { get; set; }
    }

    public class AssetBinary
    {
        public string Name { get; set; }
    }
}
