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
        void ITextExporter.Export(MessagesModel messages, TextWriter writer)
        {
            new XmlSerializer(typeof(RootModel)).Serialize(
                writer,
                new RootModel
                {
                    Message = messages
                        .Select(
                            source => new Message
                            {
                                Id = source.Id,
                                Text = source.Text,
                                Title = source.Title,
                            }
                        )
                        .ToArray()
                }
            );
        }

        (string, string[]) ITextExporter.Filter() => ("XML", "xml".Split(';'));

        [XmlRoot("Messages")]
        public class RootModel
        {
            [XmlElement]
            public Message[] Message { get; set; }
        }

        public class Message
        {
            [XmlElement]
            public int Id { get; set; }

            [XmlElement]
            public string Text { get; set; }

            [XmlElement]
            public string Title { get; set; }
        }
    }
}
