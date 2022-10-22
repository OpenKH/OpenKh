using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;
using YamlDotNet.Serialization;

namespace OpenKh.Kh2.Events.EventModels
{
    public class ReadCtrl : IEventObject
    {
        public static ushort Type => 36;

        [Data, YamlIgnore] public short cnt { get; set; }
        [Data] public short start_frame { get; set; }
        [Data] public short end_frame { get; set; }
        [Data] public short dummy { get; set; }

        [YamlIgnore] public IEventObject[] ctrls_object { get; set; }

        public EventRoot[] ctrls
        {
            get => SerializeHelper.ToEventRoots(ctrls_object);
            set => ctrls_object = SerializeHelper.FromEventRoots(value);
        }
    }
}
