using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    public interface ITextImporter
    {
        IEnumerable<ExchangeableMessage> Import(TextReader reader);
        (string, string[]) Filter();
    }
}
