using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Puzz
    {
        public enum PieceSize : byte
        {
            Twelve = 0x0,
            FourtyEight = 0x1,
            Twelve2 = 0x10,
            FourtyEight2 = 0x11
        }

        [Data] public byte Id { get; set; }
        [Data] public PieceSize Pieces { get; set; }
        [Data] public ushort Name { get; set; }
        [Data] public ushort RewardItem { get; set; }
        [Data(Count = 0xA)] public string FileName { get; set; }
    }
}
