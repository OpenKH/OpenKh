using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SplinePoint : IEventObject
    {
        public static ushort Type => 30;

        [Data] public short put_id { get; set; }
        [Data] public short cnt { get; set; }
        [Data] public short type { get; set; }
        [Data] public short dummy { get; set; }
        [Data] public float spline_lng { get; set; }
        public SplinePointData[] points { get; set; }

    }
}
