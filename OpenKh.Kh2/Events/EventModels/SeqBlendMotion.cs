using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqBlendMotion : IEventObject
    {
        public static ushort Type => 44;

        [Data] public ushort start_frame { get; set; }
        [Data] public ushort end_frame { get; set; }
        [Data] public ushort motion_start_frame { get; set; }
        [Data] public short loop_start { get; set; }
        [Data] public short loop_end { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public short blend_frame { get; set; }
        public string name { get; set; }

    }
}