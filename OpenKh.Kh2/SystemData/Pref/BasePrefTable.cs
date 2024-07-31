using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class BaseTableOffsetWithPaddings<T>
        where T : class
    {
        [Data] public int Count { get; set; }

        public static (List<T> Items, List<int> Offsets) ReadWithOffsets(Stream stream)
        {
            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                var header = new BaseTableOffsetWithPaddings<T>
                {
                    Count = reader.ReadInt32(),
                };

                var offsets = new List<int>();
                for (int i = 0; i < header.Count; i++)
                {
                    offsets.Add(reader.ReadInt32());
                }

                var items = new List<T>();
                foreach (var offset in offsets.Distinct())
                {
                    stream.Position = offset;
                    items.Add(BinaryMapping.ReadObject<T>(stream));
                }

                return (items, offsets);
            }
        }

        public static void WriteWithOffsets(Stream stream, IEnumerable<T> items, List<int> originalOffsets)
        {
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
            {
                var itemList = items as IList<T> ?? items.ToList();
                var offsets = new List<int>();

                var header = new BaseTableOffsetWithPaddings<T>
                {
                    Count = originalOffsets.Count
                };
                writer.Write(header.Count);

                foreach (var offset in originalOffsets)
                {
                    writer.Write(offset); // Write original offsets
                }

                var offsetMap = originalOffsets.Distinct().ToDictionary(offset => offset, offset => (int)stream.Position);

                foreach (var offset in offsetMap.Values)
                {
                    stream.Position = offset;
                    BinaryMapping.WriteObject(stream, itemList[offsetMap.Keys.ToList().IndexOf(offset)]);
                }
            }
        }
    }



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

    public class BaseTableSstm<T>
        where T : class
    {
        [Data] public int Version { get; set; }
        [Data] public int MagicCode { get; set; }

        public static List<T> Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                var header = new BaseTableSstm<T>
                {
                    Version = reader.ReadInt32(),
                    MagicCode = reader.ReadInt32()
                };

                var items = new List<T>();
                for (int i = 0; i < 95; i++) // 95 float values
                {
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

                var header = new BaseTableSstm<T>
                {
                    Version = 6,
                    MagicCode = 0
                };
                writer.Write(header.Version);
                writer.Write(header.MagicCode);

                foreach (var item in itemList)
                {
                    BinaryMapping.WriteObject(stream, item);
                }
            }
        }
    }
}
