using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class BgmData : IEventObject
    {
        public static ushort Type => 47;

        [Data] public short put_id { get; set; }
        [Data] public short bgm_number { get; set; }

    }
}