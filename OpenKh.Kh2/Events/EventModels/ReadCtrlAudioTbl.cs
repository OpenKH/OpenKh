using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class ReadCtrlAudioTbl : IEventObject
    {
        public static ushort Type => 38;

        public string voicenumber { get; set; }

    }
}