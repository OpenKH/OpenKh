using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class EffectDelete : IEventObject
    {
        public static ushort Type => 63;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }

    }
}