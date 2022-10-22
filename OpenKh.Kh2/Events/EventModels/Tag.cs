using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Tag : IEventObject
    {
        public static ushort Type => 81;

        [Data] public short start_frame { get; set; }
        [Data] public short unk { get; set; }
        [Data] public int tagNumber { get; set; }

    }
}