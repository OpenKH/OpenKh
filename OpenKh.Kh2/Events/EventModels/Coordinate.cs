using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Coordinate
    {
        [Data] public float x { get; set; }
        [Data] public float y { get; set; }
        [Data] public float z { get; set; }
    }
}
