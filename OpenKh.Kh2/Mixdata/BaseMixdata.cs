using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class BaseMixdata<T> : IEnumerable<T>
        where T : class
    {
        [Data] public int MagicCode { get; set; }
        [Data] public int Version { get; set; }
        [Data] public int Count { get; set; }
        [Data] public int Padding { get; set; }
        [Data] public List<T> Items { get; set; }

        public static BaseMixdata<T> Read(Stream stream)
        {
            var baseTable = BinaryMapping.ReadObject<BaseMixdata<T>>(stream);
            baseTable.Items = Enumerable.Range(0, baseTable.Count)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
            return baseTable;
        }

        public static void Write(Stream stream, int magic, int version, List<T> items) =>
            new BaseMixdata<T>()
            {
                MagicCode = magic,
                Version = version,
                Padding = 0,
                Items = items
            }.Write(stream);

        public void Write(Stream stream)
        {
            Count = Items.Count;
            BinaryMapping.WriteObject(stream, this);
            foreach (var item in Items)
                BinaryMapping.WriteObject(stream, item);
        }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}
