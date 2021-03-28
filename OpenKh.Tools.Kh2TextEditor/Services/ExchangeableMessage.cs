using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    public class ExchangeableMessage
    {
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Text { get; set; }
    }
}
