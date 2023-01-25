using Xe.BinaryMapper;

namespace OpenKh.Kh2.Models
{
    internal class ModelMulti
    {
        // Struct any model type
        [Data] public int ModelCount { get; set; }
        [Data(Count = 3)] public int DmaTagBufferEnd { get; set; }
        [Data(Count = 3)] public int DisplayFlagEnd { get; set; }
    }
}
