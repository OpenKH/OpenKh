using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqPosMove : IEventObject
    {
        public static ushort Type => 71;

        [Data] public short put_id { get; set; }
        [Data] public short start_frame { get; set; }
        [Data] public float start_x { get; set; }
        [Data] public float start_y { get; set; }
        [Data] public float start_z { get; set; }
        [Data] public float end_x { get; set; }
        [Data] public float end_y { get; set; }
        [Data] public float end_z { get; set; }
        [Data] public float x_roll { get; set; }
        [Data] public float y_roll { get; set; }
        [Data] public float z_roll { get; set; }
        [Data] public short frame { get; set; }
        [Data] public ushort unk { get; set; }

    }
}