using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Diag
    {
        public const int MagicCode = 0x49444D4A;

        public class DataInfo
        {
            [Data] public ushort DrawProgress { get; set; }
            [Data] public ushort HideProgress { get; set; }
            [Data] public byte World { get; set; }
            [Data] public byte Count { get; set; }
            [Data] public byte Type { get; set; }
            [Data] public byte Padding { get; set; }
            [Data] public uint Address{ get; set; }
        }

        public class CharacterInfo
        {
            [Data] public ushort Label { get; set; }
            [Data] public short PosX { get; set; }
            [Data] public short PosY { get; set; }
            [Data] public byte Draw { get; set; }
            [Data] public byte Padding { get; set; }
        }

        //z_un_002a18d0
    }
}
