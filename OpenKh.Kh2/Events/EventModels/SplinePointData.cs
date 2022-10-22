using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SplinePointData
    {
        [Data] public Coordinate position { get; set; }
        [Data] public Coordinate right { get; set; }
        [Data] public Coordinate left { get; set; }
        [Data] public float lng { get; set; }
    }
}
