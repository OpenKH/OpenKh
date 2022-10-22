using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class WallClip : IEventObject
    {
        public static ushort Type => 82;

        [Data] public short start_frame { get; set; }
        [Data] public short flag { get; set; }

    }
}