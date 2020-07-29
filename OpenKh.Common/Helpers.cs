using System.IO;

namespace OpenKh.Common
{
    public class Helpers
    {
        public static int Align(int offset, int alignment)
        {
            var misalignment = offset % alignment;
            return misalignment > 0 ? offset + alignment - misalignment : offset;
        }

        public static string[] GetContent(string path, bool recursive = false)
        {
            if (File.Exists(path))
                return new string[] { path };
            if (Directory.Exists(path))
                return new string[0];

            return Directory.GetFiles(path, "*", recursive ?
                SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static string YamlSerialize<T>(T obj) =>
            new YamlDotNet.Serialization.Serializer().Serialize(obj);
        public static T YamlDeserialize<T>(string content) =>
            new YamlDotNet.Serialization.Deserializer().Deserialize<T>(content);
    }
}
