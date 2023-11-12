using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Models.BoneDictSpec
{
    public class BoneElement
    {
        /// <summary>
        /// Absolute index. FK is zero based index. IK index is after last of FK.
        /// </summary>
        [XmlAttribute] public int I { get; set; }
        [XmlAttribute] public string? Name { get; set; }
        [XmlAttribute] public int SpriteIcon { get; set; }
    }
}
