using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    internal class BaseTable<T>
        where T : class
    {
        [Data] public int Id { get; set; }
        [Data] public int Count { get; set; }

        public static List<T> Read(Stream stream)
        {
            var header = BinaryMapping.ReadObject<BaseTable<T>>(stream);
            return Enumerable.Range(0, header.Count)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
        }

        public static void Write(Stream stream, int id, IEnumerable<T> items)
        {
            var itemList = items as IList<T> ?? items.ToList();
            BinaryMapping.WriteObject(stream, new BaseTable<T>
            {
                Id = id,
                Count = itemList.Count,
            });

            foreach (var item in itemList)
                BinaryMapping.WriteObject(stream, item);
        }
    }

    public class BaseShortTable<T>
        where T : class
    {
        [Data] public short Id { get; set; }
        [Data] public short Count { get; set; }

        public static List<T> Read(Stream stream)
        {
            var header = BinaryMapping.ReadObject<BaseShortTable<T>>(stream);
            return Enumerable.Range(0, header.Count)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
        }

        public static void Write(Stream stream, int id, IEnumerable<T> items)
        {
            var itemList = items as IList<T> ?? items.ToList();
            BinaryMapping.WriteObject(stream, new BaseShortTable<T>
            {
                Id = (short)id,
                Count = (short)itemList.Count,
            });

            foreach (var item in itemList)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}