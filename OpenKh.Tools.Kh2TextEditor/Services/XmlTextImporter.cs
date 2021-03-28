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
    public class XmlTextImporter : ITextImporter
    {
        (string, string[]) ITextImporter.Filter() => ("XML", "xml".Split(';'));

        IEnumerable<ExchangeableMessage> ITextImporter.Import(TextReader reader)
        {
            var model = (RootModel)new XmlSerializer(typeof(RootModel)).Deserialize(reader);
            return model.Message;
        }

        [XmlRoot("Messages")]
        public class RootModel
        {
            [XmlElement]
            public ExchangeableMessage[] Message { get; set; }
        }
    }
}
