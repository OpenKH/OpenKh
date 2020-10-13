using OpenKh.Common;
using CsvHelper;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System;
using NPOI.SS.UserModel;
using CsvHelper.TypeConversion;
using System.ComponentModel;

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
                var book = new XSSFWorkbook();
                var sheet = book.CreateSheet("Item");

                var columnNames = lists.First().Keys.ToArray();

                Func<int, int, ICell> prepareCell = (y, x) =>
                {
                    var row = sheet.GetRow(y) ?? sheet.CreateRow(y);
                    var cell = row.GetCell(x) ?? row.CreateCell(x);
                    return cell;
                };

                var x = -1;
                var y = 0;
                foreach (var name in columnNames)
                {
                    ++x;
                    prepareCell(y, x).SetCellValue(name);
                }
                foreach (var list in lists)
                {
                    ++y;
                    x = -1;
                    foreach (var name in columnNames)
                    {
                        ++x;
                        var cell = prepareCell(y, x);
                        var cellValue = list[name];
                        if (cellValue is int)
                        {
                            cell.SetCellValue((int)cellValue);
                        }
                        if (cellValue is uint)
                        {
                            cell.SetCellValue((uint)cellValue);
                        }
                        else if (cellValue is byte)
                        {
                            cell.SetCellValue((byte)cellValue);
                        }
                        else if (cellValue is short)
                        {
                            cell.SetCellValue((short)cellValue);
                        }
                        else if (cellValue is ushort)
                        {
                            cell.SetCellValue((ushort)cellValue);
                        }
                        else if (cellValue is float)
                        {
                            cell.SetCellValue((float)cellValue);
                        }
                        else if (cellValue is double)
                        {
                            cell.SetCellValue((double)cellValue);
                        }
                        else
                        {
                            cell.SetCellValue("" + cellValue);
                        }
                    }
                }

                sheet.CreateFreezePane(0, 1);

                File.Create(fileName).Using(book.Write);
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