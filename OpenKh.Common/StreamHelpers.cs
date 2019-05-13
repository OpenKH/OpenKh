using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Common
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

        public static int WriteList<T>(this Stream stream, IEnumerable<T> items)
            where T : class
        {
            var oldPosition = (int)stream.Position;
            foreach (var item in items)
                BinaryMapping.WriteObject<T>(stream, item, oldPosition);

            return (int)stream.Position - oldPosition;
        }

        public static int Write(this Stream stream, IEnumerable<int> items)
        {
            var oldPosition = (int)stream.Position;
            var writer = new BinaryWriter(stream);
            foreach (var item in items)
                writer.Write(item);

            return (int)stream.Position - oldPosition;
        }
    }
}
