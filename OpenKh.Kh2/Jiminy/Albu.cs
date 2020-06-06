using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Albu
    {
        [Data] public byte World { get; set; } //Worl -> Id
        [Data(Count = 2)] public string Number { get; set; } //menu/<region>/jm_photo/<world_id><number>.bin
        [Data] public byte Padding { get; set; }
        [Data] public ushort Unk04 { get; set; } // Padding?, always 00
        [Data] public ushort StoryFlag { get; set; }
        [Data] public ushort Title { get; set; }
        [Data] public ushort Text { get; set; }
    }
}
