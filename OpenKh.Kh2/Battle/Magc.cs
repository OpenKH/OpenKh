using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Magc
    {
        [Data] public byte Id { get; set; }
        [Data] public byte Level { get; set; }
        [Data] public byte World { get; set; }
        [Data] public byte Padding { get; set; }
        [Data(Count = 32)] public string FileName { get; set; }
        [Data] public ushort Item { get; set; }
        [Data] public ushort Command { get; set; }
        [Data] public short GroundMotion { get; set; }
        [Data] public short GroundBlend { get; set; }
        [Data] public short FinishMotion { get; set; }
        [Data] public short FinishBlend { get; set; }
        [Data] public short AirMotion { get; set; }
        [Data] public short AirBlend { get; set; }
        [Data] public sbyte Voice { get; set; }
        [Data] public sbyte VoiceFinisher { get; set; }
        [Data] public sbyte VoiceSelf { get; set; }
        [Data] public byte Padding2 { get; set; }

        public static List<Magc> Read(Stream stream) => BaseTable<Magc>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Magc> items) =>
            BaseTable<Magc>.Write(stream, 1, items);
    }
}
