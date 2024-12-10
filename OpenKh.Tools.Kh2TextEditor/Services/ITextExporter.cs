using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    public interface ITextExporter
    {
        void Export(IEnumerable<ExchangeableMessage> messages, TextWriter writer);
        (string, string[]) Filter();
    }
}
