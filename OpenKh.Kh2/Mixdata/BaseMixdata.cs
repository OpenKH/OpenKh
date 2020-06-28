using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class BaseMixdata<T> : IEnumerable<T>
    {
        [Data] public int MagicCode { get; set; }
        [Data] public int Version { get; set; }
        [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
        [Data] public int Padding { get; set; }
        [Data] public List<T> Items { get; set; }

        static BaseMixdata() => BinaryMapping.SetMemberLengthMapping<BaseMixdata<T>>(nameof(Items), (o, m) => o.Count);

        public static BaseMixdata<T> Read(Stream stream) => BinaryMapping.ReadObject<BaseMixdata<T>>(stream);
        public static void Write(Stream stream, int magic, int version, List<T> items) =>
            new BaseMixdata<T>()
            {
                MagicCode = magic,
                Version = version,
                Padding = 0,
                Items = items
            }.Write(stream);
        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}
