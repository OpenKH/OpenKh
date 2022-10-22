using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Audio : IEventObject
    {
        public static ushort Type => 35;

        [Data] public short cnt { get; set; }
        [Data] public short dummy { get; set; }
        public AudioFile[] files { get; set; }

    }
}
