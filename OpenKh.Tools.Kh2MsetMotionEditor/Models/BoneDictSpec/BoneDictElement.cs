using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Models.BoneDictSpec
{
    [XmlRoot("BoneDict")]
    public class BoneDictElement
    {
        [XmlElement("BoneView")] public BoneViewElement[]? BoneView { get; set; }

        [XmlAttribute("noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string xsiNoNamespaceSchemaLocation = "BoneDict.xsd";

    }
}
