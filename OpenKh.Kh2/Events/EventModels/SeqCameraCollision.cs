using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqCameraCollision : IEventObject
    {
        public static ushort Type => 70;

        [Data] public short start_frame { get; set; }
        [Data] public short type { get; set; }

    }
}