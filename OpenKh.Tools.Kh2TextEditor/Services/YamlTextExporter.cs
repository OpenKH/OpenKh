using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    class YamlTextExporter : ITextExporter
    {
        void ITextExporter.Export(IEnumerable<ExchangeableMessage> messages, TextWriter writer)
        {
            new YamlDotNet.Serialization.SerializerBuilder()
                .IgnoreFields()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Serialize(
                    writer,
                    messages.Select(x => new
                    {
                        id = x.Id,
                        text = x.Text
                    })
                );
        }

        (string, string[]) ITextExporter.Filter() => ("YAML", "yml".Split(';'));
    }
}
