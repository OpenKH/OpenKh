using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    public class XmlTextExporter : ITextExporter
    {
        void ITextExporter.Export(IEnumerable<ExchangeableMessage> messages, TextWriter writer)
        {
            new XmlSerializer(typeof(RootModel)).Serialize(
                writer,
                new RootModel
                {
                    Message = messages.ToArray()
                }
            );
        }

        (string, string[]) ITextExporter.Filter() => ("XML", "xml".Split(';'));

        [XmlRoot("Messages")]
        public class RootModel
        {
            [XmlElement]
            public ExchangeableMessage[] Message { get; set; }
        }
    }
}
