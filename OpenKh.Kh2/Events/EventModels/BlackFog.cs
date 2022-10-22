using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class BlackFog : IEventObject
    {
        public static ushort Type => 72;

        [Data] public short start_frame { get; set; }
        [Data] public short frame { get; set; }
        [Data] public float start { get; set; }
        [Data] public float end { get; set; }

    }
}