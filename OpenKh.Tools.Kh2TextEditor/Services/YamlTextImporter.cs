using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    class YamlTextImporter : ITextImporter
    {
        (string, string[]) ITextImporter.Filter() => ("YAML", "yml".Split(';'));

        IEnumerable<ExchangeableMessage> ITextImporter.Import(TextReader reader)
        {
            var model = new YamlDotNet.Serialization.DeserializerBuilder()
                .Build()
                .Deserialize<RootModel>(reader);

            return model.Messages;
        }

        public class RootModel
        {
            public ExchangeableMessage[] Messages { get; set; }
        }
    }
}
