using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2.Battle
{
    /// <summary>
    /// Unfinished
    /// </summary>
    public class Atkp
    {
        public int Id { get; set; }
        public short Unknown04 { get; set; }
        public short Unknown06 { get; set; }
        public short Unknown08 { get; set; }
        public short Unknown0a { get; set; }
        public short Unknown0c { get; set; }
        public short Unknown0e { get; set; }
        public short Unknown10 { get; set; }
        public short Unknown12 { get; set; }
        public short Unknown14 { get; set; }
        public short Unknown16 { get; set; }
        public short Unknown18 { get; set; }
        public short Unknown1a { get; set; }
        public int Unknown1c { get; set; }
        public short Unknown20 { get; set; }
        public short Unknown22 { get; set; }
        public short Unknown24 { get; set; }
        public short Unknown26 { get; set; }
        public short Unknown28 { get; set; }
        public short Unknown2a { get; set; }
        public short Unknown2c { get; set; }
        public short Unknown2e { get; set; }

        public static List<Atkp> Read(Stream stream) => BaseTable<Atkp>.Read(stream);
    }
}
