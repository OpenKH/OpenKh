using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;
using YamlDotNet.Serialization;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeSeq : IEventObject
    {
        public static ushort Type => 43;

        [Data] public short put_id { get; set; }
        [Data] public short type { get; set; }
        [Data] public int se_number { get; set; }
        [Data] public short start_frame { get; set; }
        [Data, YamlIgnore] public short padding { get; set; }

    }
}
