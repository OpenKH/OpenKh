using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class BaseJiminy<T> : IEnumerable<T>
    {
        [Data] public int MagicCode { get; set; }
        [Data] public int Version { get; set; }
        [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
        [Data] public int Padding { get; set; }
        [Data] public List<T> Items { get; set; }

        static BaseJiminy() => BinaryMapping.SetMemberLengthMapping<BaseJiminy<T>>(nameof(Items), (o, m) => o.Count);

        public static BaseJiminy<T> Read(Stream stream) => BinaryMapping.ReadObject<BaseJiminy<T>>(stream);
        public static void Write(Stream stream, int magic, List<T> items) =>
            new BaseJiminy<T>()
            {
                MagicCode = magic,
                Version = 12,
                Padding = 0,
                Items = items
            }.Write(stream);
        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}
