using Xe.BinaryMapper;


namespace OpenKh.Audio
{
    public partial class Scd
    {
        public class MsadpcmExtraData
        {
            //WAVEFORMATEX
            [Data] public short FormatTag { get; set; }
            [Data] public short Channels { get; set; }
            [Data] public int SamplesPerSecond{ get; set; }
            [Data] public int AverageBytesPerSecond{ get; set; }
            [Data] public short BlockAlignment{ get; set; }
            [Data] public short BitsPerSample{ get; set; }
            [Data] public short Size { get; set; }
            [Data] public short Unk12 { get; set; }
            [Data] public ushort CoefCount { get; set; } //default 7
            [Data(Count = 14)] public short[] Coefs { get; set; } //Coefs are in pairs => 7*2 = 14
        }
    }
}
