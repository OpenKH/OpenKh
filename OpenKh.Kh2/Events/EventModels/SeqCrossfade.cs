using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqCrossfade : IEventObject
    {
        public static ushort Type => 27;

        [Data] public short start_frame { get; set; }
        [Data] public short fade_frame { get; set; }

    }
}