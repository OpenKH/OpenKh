using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Objentry
    {
        [Data] public ushort ObjectId { get; set; }
        [Data] public ushort Unknown02 { get; set; }
        [Data] public ushort ObjectType { get; set; }
        [Data] public byte Unknown06 { get; set; }
        [Data] public byte WeaponJoint { get; set; }
        [Data(Count = 32)] public byte[] ModelName { get; set; }
        [Data(Count = 32)] public byte[] AnimationName { get; set; }
        [Data] public uint Unknown48 { get; set; }
        [Data] public ushort NeoStatus { get; set; }
        [Data] public ushort NeoMoveset { get; set; }
        [Data] public uint Unknown50 { get; set; }
        [Data] public uint Unknown54 { get; set; }
        [Data] public uint Unknown58 { get; set; }
        [Data] public uint Unknown5c { get; set; }

        public static BaseTable<Objentry> Read(Stream stream) => BaseTable<Objentry>.Read(stream);
    }
}
