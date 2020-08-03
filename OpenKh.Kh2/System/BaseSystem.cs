using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class BaseSystem<T>
        where T : class
    {
        [Data] public short Id { get; set; }
        [Data] public short Count { get; set; }
        [Data] public List<T> Items { get; set; }

        public static BaseSystem<T> Read(Stream stream)
        {
            var baseTable = BinaryMapping.ReadObject<BaseSystem<T>>(stream.SetPosition(0));
            baseTable.Items = Enumerable.Range(0, baseTable.Count - 1)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
            return baseTable;
        }

        public void Write(Stream stream)
        {
            Count = (short)Items.Count;
            BinaryMapping.WriteObject(stream, this);
            foreach (var item in Items)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}