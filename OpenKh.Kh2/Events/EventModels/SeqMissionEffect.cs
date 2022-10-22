using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqMissionEffect : IEventObject
    {
        public static ushort Type => 60;

        [Data] public short start_frame { get; set; }
        [Data] public short number { get; set; }
        [Data] public short end_type { get; set; }

    }
}