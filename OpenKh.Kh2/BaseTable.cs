using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class BaseTable<T> : IEnumerable<T>
    {
        [Data] public int Id { get; set; }
        [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
        [Data] public List<T> Items { get; set; }

        static BaseTable() => BinaryMapping.SetMemberLengthMapping<BaseTable<T>>(nameof(Items), (o, m) => o.Count);

        public static BaseTable<T> Read(Stream stream) => BinaryMapping.ReadObject<BaseTable<T>>(stream);
        public static void Write(Stream stream, int id, List<T> items) =>
            new BaseTable<T>()
            {
                Id = id,
                Items = items
            }.Write(stream);


        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}