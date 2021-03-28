using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    public class TextExporters
    {
        public static IEnumerable<ITextExporter> GetAll() => new ITextExporter[]
        {
            new PlainTextExporter(),
            new XmlTextExporter(),
            new YamlTextExporter(),
        };

        public static ITextExporter FindFromFile(string fileName)
        {
            var selectedExtension = Path.GetExtension(fileName).TrimStart('.');
            var textExporters = GetAll();

            return textExporters
                .Where(
                    exporter => exporter.Filter().Item2
                        .Any(
                            it => string.Compare(it, selectedExtension, true) == 0
                        )
                )
                .FirstOrDefault() ?? textExporters.First(); // fallback
        }
    }
}
