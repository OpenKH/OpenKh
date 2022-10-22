using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class TexFade : IEventObject
    {
        public static ushort Type => 33;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public float from { get; set; }
        [Data] public float to { get; set; }
        [Data] public float frame { get; set; }

    }
}