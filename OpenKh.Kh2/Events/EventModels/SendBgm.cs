using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SendBgm : IEventObject
    {
        public static ushort Type => 48;

        [Data] public short start_frame { get; set; }
        [Data] public short bgm_bank { get; set; }
        [Data] public short bgm_number { get; set; }

    }
}