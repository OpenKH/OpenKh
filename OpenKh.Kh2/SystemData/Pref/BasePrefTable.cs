using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class BaseTableOffsets<T>
        where T : class
    {
        [Data] public int Count { get; set; }

        public static List<T> Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                var header = new BaseTableOffsets<T>
                {
                    Count = reader.ReadInt32()
                };

                var offsets = new int[header.Count];
                for (int i = 0; i < header.Count; i++)
                {
                    offsets[i] = reader.ReadInt32();
                }

                var items = new List<T>();
                foreach (var offset in offsets)
                {
                    stream.Position = offset;
                    items.Add(BinaryMapping.ReadObject<T>(stream));
                }
                return items;
            }
        }

        public static void Write(Stream stream, IEnumerable<T> items)
        {
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
            {
                var itemList = items as IList<T> ?? items.ToList();
                var offsets = new List<int>();

                var header = new BaseTableOffsets<T>
                {
                    Count = itemList.Count
                };
                writer.Write(header.Count);

                var offsetPosition = stream.Position;
                for (int i = 0; i < itemList.Count; i++)
                {
                    writer.Write(0); // Placeholder for offset
                }

                for (int i = 0; i < itemList.Count; i++)
                {
                    offsets.Add((int)stream.Position);
                    BinaryMapping.WriteObject(stream, itemList[i]);
                }

                var currentPosition = stream.Position;
                stream.Position = offsetPosition;
                foreach (var offset in offsets)
                {
                    writer.Write(offset);
                }
                stream.Position = currentPosition;
            }
        }
    }
}
