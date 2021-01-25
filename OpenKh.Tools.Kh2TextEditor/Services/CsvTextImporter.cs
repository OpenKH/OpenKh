using CsvHelper;
using CsvHelper.Configuration;
using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2TextEditor.Services
{
    class CsvTextImporter : ITextImporter
    {
        (string, string[]) ITextImporter.Filter() => ("CSV", "csv".Split(';'));

        IEnumerable<ExchangeableMessage> ITextImporter.Import(TextReader reader)
        {
            return new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
            })
                .GetRecords<ExchangeableMessage>();
        }
    }
}
