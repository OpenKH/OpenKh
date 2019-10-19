using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class BaseSystem<T>
    {
        [Data] public short Id { get; set; }
        [Data] public short Count { get => (short)Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
        [Data] public List<T> Items { get; set; }

        static BaseSystem() => BinaryMapping.SetMemberLengthMapping<BaseSystem<T>>(nameof(Items), (o, m) => o.Count);

        public static BaseSystem<T> Read(Stream stream) => BinaryMapping.ReadObject<BaseSystem<T>>(stream);

        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);
    }
}