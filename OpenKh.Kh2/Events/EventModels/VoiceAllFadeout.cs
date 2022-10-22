using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class VoiceAllFadeout : IEventObject
    {
        public static ushort Type => 83;

        [Data] public short start_frame { get; set; }
        [Data] public short fade_frame { get; set; }

    }
}