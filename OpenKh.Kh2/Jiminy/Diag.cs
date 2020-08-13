using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Diag
    {
        public const int MagicCode = 0x49444D4A;

        public class Flag
        {
            [Data] public ushort Id { get; set; }
            [Data] public ushort Unk02 { get; set; }
            [Data] public byte World { get; set; }
            [Data] public byte Room { get; set; }
            [Data] public byte Unk06 { get; set; }
            [Data] public byte Padding { get; set; }
            [Data] public int Unk08 { get; set; }
        }

        //z_un_002a18d0
    }
}
