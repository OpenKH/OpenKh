using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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
        public string OriginalAuthor { get; set; }
        public string Description { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Game { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public int Specifications { get; set; }
        public List<Dependency> Dependencies { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public bool IsCollection { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public List<string> CollectionGames { get; set; }
        public List<AssetFile> Assets { get; set; }

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


        public static Metadata Read(Stream stream)
        {
            try
            {
                return deserializer.Deserialize<Metadata>(new StreamReader(stream)); //Simplifed way of doing this. Cuts out the "deserializer" redundancies and variable for StreamReader(stream))
            }

            catch (YamlDotNet.Core.YamlException ex)
            {
                // Handle YAML parsing errors
                Debug.WriteLine($"Error deserializing YAML: {ex.Message}");

                string originalTitle = string.Empty;

                // Extract title using regex
                stream.Position = 0; // Reset stream position
                using (var reader = new StreamReader(stream))
                {
                    string yamlContent = reader.ReadToEnd();
                    var match = Regex.Match(yamlContent, @"(?<=title:).*");
                    if (match.Success)
                    {
                        originalTitle = match.Value.Trim();
                    }
                }

                var metadata = new Metadata
                {
                    Title = $"{originalTitle}* \nMOD YML ERROR DETECTED: CHECK FORMATTING"
                };

                return metadata; // Return modified metadata indicating failure
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                throw; // Rethrow other exceptions for further investigation
            }
        }

        public void Write(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                serializer.Serialize(writer, this);
            }
        }
        public override string ToString() =>
        serializer.Serialize(this);
    }

    public class AssetFile
    {
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Name { get; set; }

        /// <summary>
        /// "areadatascript"
        /// "bdscript"
        /// "binarc"
        /// "copy"
        /// "imgd"
        /// "imgz"
        /// "kh2msg"
        /// "listpatch"
        /// "spawnpoint"
        /// "synthpatch"
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Method { get; set; }

        /// <summary>
        /// null
        /// ""
        /// "both"
        /// "pc"
        /// "ps2"
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Platform { get; set; }

        /// <summary>
        /// (null)
        /// bbs_first
        /// bbs_fourth
        /// bbs_second
        /// bbs_third
        /// kh1_fifth
        /// kh1_first
        /// kh1_fourth
        /// kh1_second
        /// kh1_third
        /// kh2_fifth
        /// kh2_first
        /// kh2_fourth
        /// kh2_second
        /// kh2_sixth
        /// kh2_third
        /// kh3d_first
        /// kh3d_fourth
        /// kh3d_second
        /// kh3d_third
        /// Recom
        /// Theater
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Package { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public List<Multi> Multi { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public List<AssetFile> Source { get; set; }

        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public bool Required { get; set; }

        /// <summary>
        /// "areadatascript"
        /// "areadataspawn"
        /// "atkp"
        /// "bdx"
        /// "Binary"
        /// "bons"
        /// "cmd"
        /// "condition"
        /// "enmp"
        /// "fmab"
        /// "fmlv"
        /// "imgd"
        /// "imgz"
        /// "internal"
        /// "item"
        /// "jigsaw"
        /// "level"
        /// "libretto"
        /// "list"
        /// "localset"
        /// "lvup"
        /// "magc"
        /// "memt"
        /// "objentry"
        /// "place"
        /// "plrp"
        /// "przt"
        /// "recipe"
        /// "sklt"
        /// "soundinfo"
        /// "Synthesis"
        /// "trsr"
        /// "vtbl"
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Type { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public Bar.MotionsetType MotionsetType { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Language { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public bool IsSwizzled { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public int Index { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public string Game { get; set; }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)] public bool CollectionOptional { get; set; }
    }

    public class Multi
    {
        public string Name { get; set; }
    }
}
