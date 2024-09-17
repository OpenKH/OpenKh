using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Localset
    {
        [Data] public ushort ProgramId { get; set; }
        [Data] public ushort MapNumber { get; set; }

        public static List<Localset> Read(Stream stream) => BaseTable<Localset>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Localset> entries) =>
            BaseTable<Localset>.Write(stream, 1, entries);
    }
}
