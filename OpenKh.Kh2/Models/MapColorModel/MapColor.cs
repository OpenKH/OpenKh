using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;
using Xe.Graphics;

namespace OpenKh.Kh2.Models.MapColorModel
{
    public class MapColor
    {
        [Data] public Color BgColor { get; set; }
        [Data(Count = 16)] public Color[] OnColorTable { get; set; }
        [Data] public Fog Fog { get; set; }
    }
}
