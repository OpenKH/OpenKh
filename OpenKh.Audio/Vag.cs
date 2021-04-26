using Xe.BinaryMapper;

namespace OpenKh.Audio
{
    public class Vag
    {
        public class VAGHeader
        {
            [Data] public uint Magic { get; set; }
            [Data] public uint Version { get; set; } // Usually 3
            [Data] public uint Reserved08 { get; set; }
            [Data] public uint DataSize { get; set; }
            [Data] public uint SamplingFrequency { get; set; }
            [Data(Count = 10)] public byte[] Reserved14 { get; set; }
            [Data] public byte ChannelCount { get; set; }
            [Data] public byte Reserved1F { get; set; }
            [Data(Count = 16)] public string Name { get; set; }
        }
    }
}
