using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    class YamlTextExporter : ITextExporter
    {
        void ITextExporter.Export(MessagesModel messages, TextWriter writer)
        {
            new YamlDotNet.Serialization.SerializerBuilder()
                .Build()
                .Serialize(
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

        (string, string[]) ITextExporter.Filter() => ("YAML", "yml".Split(';'));

        public class RootModel
        {
            public Message[] Message { get; set; }
        }

        public class Message
        {
            public int Id { get; set; }

            public string Text { get; set; }

            public string Title { get; set; }
        }
    }
}
