using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class CameraData : IEventObject
    {
        public static ushort Type => 5;

        [Data] public short put_id { get; set; }
        [Data] public short unk { get; set; }

    }
}