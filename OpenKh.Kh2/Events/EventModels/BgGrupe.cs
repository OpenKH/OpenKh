using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class BgGrupe : IEventObject
    {
        public static ushort Type => 22;

        [Data] public short start_frame { get; set; }
        [Data] public byte no { get; set; }
        [Data] public byte flag { get; set; }

    }
}