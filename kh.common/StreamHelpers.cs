using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace kh.common
{
    public static class StreamHelpers
    {
        public static List<T> ReadList<T>(this Stream stream, int offset, int count)
            where T : class
        {
            stream.Position = offset;
            return stream.ReadList<T>(count);
        }

        public static List<T> ReadList<T>(this Stream stream, int count)
            where T : class
        {
            return Enumerable.Range(0, count)
                .Select(x => BinaryMapping.ReadObject<T>(stream, (int)stream.Position))
                .ToList();
        }

        public static List<int> ReadInt32List(this Stream stream, int offset, int count)
        {
            stream.Position = offset;
            return stream.ReadInt32List(count);
        }

        public static List<int> ReadInt32List(this Stream stream, int count)
        {
            var reader = new BinaryReader(stream);
            return Enumerable.Range(0, count)
                .Select(x => reader.ReadInt32())
                .ToList();
        }
    }
}
