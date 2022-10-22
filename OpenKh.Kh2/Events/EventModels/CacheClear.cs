using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;
using YamlDotNet.Serialization;

namespace OpenKh.Kh2.Events.EventModels
{
    public class CacheClear : IEventObject
    {
        public static ushort Type => 64;

        [Data] public short start_frame { get; set; }
        [Data(Count = 96), YamlIgnore] public byte[] raw_put_id { get; set; }

        public string put_id
        {
            get => TextHelper.GetString(raw_put_id);
            set => TextHelper.GetBytes(value, 96);
        }
    }
}
