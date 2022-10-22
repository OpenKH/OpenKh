using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;
using YamlDotNet.Serialization;

namespace OpenKh.Kh2.Events.EventModels
{
    public class AudioFile
    {
        [Data] public int read_buff { get; set; }
        [Data] public short start_frame { get; set; }
        [Data] public byte flag { get; set; }
        [Data(Count = 24), YamlIgnore] public byte[] raw_name { get; set; }
        [Data, YamlIgnore] public byte padding { get; set; }

        public string name
        {
            get => TextHelper.GetString(raw_name);
            set => raw_name = TextHelper.GetBytes(value, 24);
        }
    }
}
