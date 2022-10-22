using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Fog : IEventObject
    {
        public static ushort Type => 73;

        [Data] public short start_frame { get; set; }
        [Data] public byte min { get; set; }
        [Data] public byte max { get; set; }
        [Data] public int fog_near { get; set; }
        [Data] public byte r { get; set; }
        [Data] public byte g { get; set; }
        [Data] public byte b { get; set; }
        [Data] public byte unk { get; set; }

    }
}