using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class VibData : IEventObject
    {
        public static ushort Type => 53;

        [Data] public short start_frame { get; set; }
        [Data] public short dummy { get; set; }
        [Data(Count = 2)] public byte[] data { get; set; }

    }
}
