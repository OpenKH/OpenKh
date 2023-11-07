using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Models.BoneDictSpec
{
    public class BoneViewElement
    {
        /// <summary>
        /// A set of pattern like `p_ex100`.
        /// 
        /// - `;` means `or else`
        /// - `,` means `and also`
        /// </summary>
        [XmlAttribute] public string? Match { get; set; }

        [XmlElement] public BoneElement[]? Bone { get; set; }
    }
}
