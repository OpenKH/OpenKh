using System.IO;
using System.Runtime.InteropServices;

namespace OpenKh.Common
{
    public class Helpers
    {
        public static int Align(int offset, int alignment)
        {
            var misalignment = offset % alignment;
            return misalignment > 0 ? offset + alignment - misalignment : offset;
        }

        public static long Align(long offset, int alignment)
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

        public static byte[] ToRawData<T>(T input)
        {
            var size = Marshal.SizeOf(input);
            var bytes = new byte[size];

            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(input, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);

            Marshal.FreeHGlobal(ptr);

            return bytes;
        }

        public static T FromRawData<T>(byte[] input)
        {
            var size = input.Length;

            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(input, 0, ptr, size);

            var data = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return data;
        }
    }
}
