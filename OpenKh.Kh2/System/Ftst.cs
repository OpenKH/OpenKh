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

        public static List<Entry> Read(Stream stream) =>
            BaseTable<Entry>.Read(stream);

        public static void Write(Stream stream, List<Entry> entries) =>
            BaseTable<Entry>.Write(stream, 1, entries);
    }
}
