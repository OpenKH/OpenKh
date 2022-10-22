using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqBgse : IEventObject
    {
        public static ushort Type => 66;

        [Data] public short start_frame { get; set; }
        [Data] public byte sw { get; set; }
        [Data] public byte dummy { get; set; }

    }
}