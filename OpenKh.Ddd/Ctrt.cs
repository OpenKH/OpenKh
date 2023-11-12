using Xe.BinaryMapper;

namespace OpenKh.Ddd
{
    public class Ctrt
    {
        public CtrtHeader Header { get; set; }
        public byte[] EncryptedData { get; set; }

        // Encoded with Etc1 and swizzled
        public class CtrtHeader
        {
            [Data] public int Magic { get; set; } //CTRT
            [Data] public int Unk04 { get; set;}
            [Data] public int Unk08 { get; set;}
            [Data] public int DataOffset { get; set; }
            [Data] public int Unk10 { get; set; }
            [Data] public int DataSize { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public int Format { get; set; }
            [Data] public short Width { get; set; }
            [Data] public short Height { get; set; }
            [Data] public int Unk24 { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public int Unk2C { get; set; }
            [Data(Count =80)] public byte[] UnknownData { get; set; }
        }

        enum TextureFormat
        {
            RGBA_8888 = 0,
            RGB_888 = 1,
            RGBA_5551 = 2,
            RGB_565 = 3,
            RGBA_4444 = 4,
            LA8 = 5,
            HILO8 = 6,
            L8 = 7,
            A8 = 8,
            LA4 = 9,
            L4 = 10,
            A4 = 11,
            ETC1 = 12,
            ETC1A4 = 13
        }
    }
}
