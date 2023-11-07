using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Models.Presets
{
    [XmlRoot("Presets")]
    public class MdlxMsetPresets
    {
        [XmlElement("Preset")] public MdlxMsetPreset[]? Preset { get; set; }

        public IEnumerable<MdlxMsetPreset> GetPresets() => Preset ?? Enumerable.Empty<MdlxMsetPreset>();

        [XmlAttribute("noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string xsiNoNamespaceSchemaLocation = "Presets.xsd";
    }
}
