using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    public interface ITextImporter
    {
        IEnumerable<ExchangeableMessage> Import(TextReader reader);
        (string, string[]) Filter();
    }
}
