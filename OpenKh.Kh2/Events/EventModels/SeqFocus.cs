using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqFocus : IEventObject
    {
        public static ushort Type => 24;

        [Data] public short start_frame { get; set; }
        [Data] public byte sw { get; set; }
        [Data] public byte type { get; set; }
        [Data] public int z { get; set; }

    }
}