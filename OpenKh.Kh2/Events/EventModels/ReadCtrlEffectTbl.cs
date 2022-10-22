using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class ReadCtrlEffectTbl : IEventObject
    {
        public static ushort Type => 57;

        [Data] public short put_id { get; set; }
        public string name { get; set; }

    }
}