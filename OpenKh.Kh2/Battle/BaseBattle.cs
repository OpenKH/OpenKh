using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class BaseBattle<T>
    {
        [Data] public int Id { get; set; }
        [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
        [Data] public List<T> Items { get; set; }

        static BaseBattle() => BinaryMapping.SetMemberLengthMapping<BaseBattle<T>>(nameof(Items), (o, m) => o.Count);

        public static BaseBattle<T> Read(Stream stream) => BinaryMapping.ReadObject<BaseBattle<T>>(stream);

        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);
    }
}
