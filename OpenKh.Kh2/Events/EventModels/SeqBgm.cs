using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqBgm : IEventObject
    {
        public static ushort Type => 46;

        [Data] public short start_frame { get; set; }
        [Data] public short volume_continue { get; set; }
        [Data] public byte start_volume { get; set; }
        [Data] public byte end_volume { get; set; }
        [Data] public byte fade_frame { get; set; }
        [Data] public byte bank { get; set; }

    }
}