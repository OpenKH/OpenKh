using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class Ftst
    {
        public class Entry
        {
            [Data] public int Id { get; set; }
            [Data(Count = Constants.WorldCount)] public int[] Colors { get; set; }
        }

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Count { get => Entries.TryGetCount(); set => Entries = Entries.CreateOrResize(value); }
            [Data] public List<Entry> Entries { get; set; }
        }

        static Ftst()
        {
            BinaryMapping.SetMemberLengthMapping<Header>(nameof(Header.Entries), (o, m) => o.Count);
        }

        public static List<Entry> Read(Stream stream) =>
            BinaryMapping.ReadObject<Header>(stream).Entries;

        public static void Write(Stream stream, List<Entry> entries) =>
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = 1,
                Entries = entries
            });
    }
}
