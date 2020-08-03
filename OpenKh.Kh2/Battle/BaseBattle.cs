using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class BaseBattle<T>
        where T : class
    {
        [Data] public int Id { get; set; }
        [Data] public int Count { get; set; }
        public List<T> Items { get; set; }

        public static BaseBattle<T> Read(Stream stream)
        {
            var baseTable = BinaryMapping.ReadObject<BaseBattle<T>>(stream);
            baseTable.Items = Enumerable.Range(0, baseTable.Count)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
            return baseTable;
        }

        public void Write(Stream stream)
        {
            Count = Items.Count;
            BinaryMapping.WriteObject(stream, this);
            foreach (var item in Items)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}
