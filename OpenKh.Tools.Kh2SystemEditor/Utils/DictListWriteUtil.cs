using CsvHelper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Tools.Kh2SystemEditor.Utils
{
    class DictListWriteUtil
    {
        internal static void Write(string fileName, IList<IDictionary<string, object>> lists)
        {
            var fileExt = Path.GetExtension(fileName).ToLowerInvariant();

            if (fileExt == ".yml")
            {
                File.WriteAllText(
                    fileName,
                    new YamlDotNet.Serialization.SerializerBuilder()
                        .Build()
                        .Serialize(lists)
                );
            }
            else if (fileExt == ".xlsx")
            {
                using var xlsx = new ExcelPackage();
                var book = xlsx.Workbook;
                var sheet = book.Worksheets.Add("Item");

                var columnNames = lists.First().Keys.ToArray();

                var x = 0;
                var y = 1;
                foreach (var name in columnNames)
                {
                    ++x;
                    sheet.Cells[y, x].Value = name;
                }
                foreach (var list in lists)
                {
                    ++y;
                    x = 0;
                    foreach (var name in columnNames)
                    {
                        ++x;
                        sheet.Cells[y, x].Value = list[name];
                    }
                }

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                xlsx.SaveAs(new FileInfo(fileName));
            }
            else
            {
                // csv
                using var writer = new StreamWriter(fileName, false, new UTF8Encoding(true));
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                if (lists.Any())
                {
                    var columnNames = lists.First().Keys.ToArray();

                    foreach (var name in columnNames)
                    {
                        csv.WriteField(name);
                    }
                    foreach (var list in lists)
                    {
                        csv.NextRecord();

                        foreach (var name in columnNames)
                        {
                            csv.WriteField(list[name]);
                        }
                    }
                }
            }
        }
    }
}