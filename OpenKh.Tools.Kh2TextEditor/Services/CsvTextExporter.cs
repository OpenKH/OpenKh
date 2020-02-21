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
    class CsvTextExporter : ITextExporter
    {
        void ITextExporter.Export(MessagesModel messages, TextWriter writer)
        {
            new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ","
            })
                .WriteRecords(
                    messages
                        .Select(
                            source => new Message
                            {
                                Id = source.Id,
                                Text = source.Text,
                                Title = source.Title,
                            }
                        )
                        .ToArray()
                );
        }

        (string, string[]) ITextExporter.Filter() => ("CSV", "csv".Split(';'));

        public class Message
        {
            public int Id { get; set; }

            public string Text { get; set; }

            public string Title { get; set; }
        }
    }
}
