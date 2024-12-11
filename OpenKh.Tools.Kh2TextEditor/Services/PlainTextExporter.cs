using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    class PlainTextExporter : ITextExporter
    {
        void ITextExporter.Export(IEnumerable<ExchangeableMessage> messages, TextWriter writer)
        {
            foreach (var one in messages)
            {
                writer.WriteLine($"{one.Id}: {one.Text}");
                writer.WriteLine("---");
            }
        }

        (string, string[]) ITextExporter.Filter() => ("Plain text", "txt".Split(';'));
    }
}
