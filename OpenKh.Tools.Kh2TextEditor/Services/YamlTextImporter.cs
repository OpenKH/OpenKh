using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    class YamlTextImporter : ITextImporter
    {
        (string, string[]) ITextImporter.Filter() => ("YAML", "yml".Split(';'));

        IEnumerable<ExchangeableMessage> ITextImporter.Import(TextReader reader)
        {
            var messages = new YamlDotNet.Serialization.DeserializerBuilder()
                .IgnoreFields()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<MessageModel[]>(reader);

            return messages.Select(x => new ExchangeableMessage
            {
                Id = x.id,
                Text = x.text
            });
        }

        public class MessageModel
        {
            public int id { get; set; }
            public string text { get; set; }
        }
    }
}
