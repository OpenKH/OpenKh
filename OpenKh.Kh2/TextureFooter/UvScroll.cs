using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.TextureFooter
{
    public class UvScroll
    {
        [Data] public int TextureIndex { get; set; }
        [Data] public float UScrollSpeed { get; set; }
        [Data] public float VScrollSpeed { get; set; }

        public override string ToString() => $"Idx={TextureIndex}, USpd={UScrollSpeed}, VSpd={VScrollSpeed}";
    }
}
