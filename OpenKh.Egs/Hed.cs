using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Egs
{
    public class Hed
    {
        public class Entry
        {
            [Data(Count = 16)] public byte[] MD5 { get; set; }
            [Data] public long Offset { get; set; }
            [Data] public int DataLength { get; set; }
            [Data] public int ActualLength { get; set; }
        }

        public static IEnumerable<Entry> Read(Stream stream) => Enumerable
            .Range(0, (int)(stream.Length / 0x20))
            .Select(_ => BinaryMapping.ReadObject<Entry>(stream));
    }
}
