using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class BaseTable<T> : IEnumerable<T>
        where T : class
    {
        [Data] public int Id { get; set; }
        [Data] public int Count { get; set; }
        public List<T> Items { get; set; }

        public static BaseTable<T> Read(Stream stream)
        {
            var baseTable = BinaryMapping.ReadObject<BaseTable<T>>(stream);
            baseTable.Items = Enumerable.Range(0, baseTable.Count)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
            return baseTable;
        }

        public static void Write(Stream stream, int id, List<T> items) =>
            new BaseTable<T>()
            {
                Id = id,
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

    public class BaseShortTable<T> : IEnumerable<T>
        where T : class
    {
        [Data] public short Id { get; set; }
        [Data] public short Count { get; set; }
        public List<T> Items { get; set; }

        public static BaseShortTable<T> Read(Stream stream)
        {
            var baseTable = BinaryMapping.ReadObject<BaseShortTable<T>>(stream);
            baseTable.Items = Enumerable.Range(0, baseTable.Count)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
            return baseTable;
        }

        public static void Write(Stream stream, int id, List<T> items) =>
            new BaseShortTable<T>()
            {
                Id = (short)id,
                Items = items
            }.Write(stream);


        public void Write(Stream stream)
        {
            Count = (short)Items.Count;
            BinaryMapping.WriteObject(stream, this);
            foreach (var item in Items)
                BinaryMapping.WriteObject(stream, item);
        }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}