using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class BaseTable<T>
    {
        [Data] public int Id { get; set; }
        [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
        [Data] public List<T> Items { get; set; }

        static BaseTable() => BinaryMapping.SetMemberLengthMapping<BaseTable<T>>(nameof(Items), (o, m) => o.Count);

        public static BaseTable<T> Read(Stream stream) => BinaryMapping.ReadObject<BaseTable<T>>(stream);

        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);
    }
}