using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqPosi : IEventObject
    {
        public static ushort Type => 2;

        [Data] public ushort sub_id { get; set; }
        [Data] public ushort unk { get; set; }
        [Data] public float x { get; set; }
        [Data] public float y { get; set; }
        [Data] public float z { get; set; }
        [Data] public float x_roll { get; set; }
        [Data] public float y_roll { get; set; }
        [Data] public float z_roll { get; set; }
        [Data] public float scale { get; set; }
        [Data] public ushort put_id { get; set; }
        [Data] public ushort start_frame { get; set; }
    }
}
