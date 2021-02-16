using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OpenKh.Research.Kh2Anim.Models
{
    [XmlRoot]
    public class MotionExport
    {
        [XmlElement]
        public FrameClass[] Frame;

        [XmlAttribute]
        public float FrameLoop;
        [XmlAttribute]
        public float FrameEnd;
        [XmlAttribute]
        public float FramePerSecond;
        [XmlAttribute]
        public float FrameCount;
        [XmlAttribute]
        public int MatrixCount;

        public class FrameClass
        {
            [XmlAttribute]
            public float Time;

            [XmlElement]
            public string Matrices;
        }
    }
}
