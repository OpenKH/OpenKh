using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
