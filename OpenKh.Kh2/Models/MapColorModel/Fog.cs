using Xe.BinaryMapper;
using Xe.Graphics;

namespace OpenKh.Kh2.Models.MapColorModel
{
    public class Fog
    {
        [Data] public Color FogColor { get; set; }
        [Data] public float Min { get; set; }
        [Data] public float Max { get; set; }
        [Data] public float Near { get; set; }
        [Data] public float Far { get; set; }
    }
}
