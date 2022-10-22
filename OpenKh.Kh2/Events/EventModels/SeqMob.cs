using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqMob : IEventObject
    {
        public static ushort Type => 79;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public float x { get; set; }
        [Data] public float y { get; set; }
        [Data] public float z { get; set; }
        [Data] public int num { get; set; }
        [Data] public float range_x { get; set; }
        [Data] public float range_y { get; set; }
        [Data] public float range_z { get; set; }
        [Data] public float rot_y { get; set; }

    }
}