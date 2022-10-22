using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class WideMask : IEventObject
    {
        public static ushort Type => 34;

        [Data] public short start_frame { get; set; }
        [Data] public short flag { get; set; }

    }
}