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
        void ITextExporter.Export(IEnumerable<ExchangeableMessage> messages, TextWriter writer)
        {
            new YamlDotNet.Serialization.SerializerBuilder()
                .Build()
                .Serialize(
                    writer,
                    new
                    {
                        Messages = messages
                    }
                );
        }

        (string, string[]) ITextExporter.Filter() => ("YAML", "yml".Split(';'));
    }
}
