using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Models.Presets
{
    public class MdlxMsetPreset
    {
        [XmlAttribute] public string? Label { get; set; }
        [XmlAttribute] public string? Mdlx { get; set; }
        [XmlAttribute] public string? Mset { get; set; }
        [XmlAttribute] public bool AutoLoad { get; set; }
        [XmlAttribute] public string? DefaultMotion { get; set; }
    }
}
