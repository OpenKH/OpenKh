using System.Collections.Generic;
using Xe.BinaryMapper;

namespace OpenKh.Audio
{
    public partial class Scd
    {
        public class OggExtraData
        {
            [Data] public short EncodeType { get; set; }
            [Data] public byte EncodeByte { get; set; }
            [Data] public byte Unk3 { get; set; }
            [Data] public uint Unk4 { get; set; }
            [Data] public uint Unk8 { get; set; }
            [Data] public uint UnkC { get; set; }
            [Data] public int SeekTableSize { get; set; }
            [Data] public int VorbisHeaderSize { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public int Padding { get; set; }

            public List<int> SeekTable { get; set; }
            public byte[] VorbisHeader { get; set; }
        }

        public static void Xor(byte[] ptr, byte key)
        {
            for (int i = 0; i < ptr.Length; i++)
                ptr[i] ^= key;
        }
    }
}
