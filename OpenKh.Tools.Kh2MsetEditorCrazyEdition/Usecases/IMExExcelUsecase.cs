using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
{
    public class IMExExcelUsecase
    {
        private readonly LoadedModel _loadedModel;

        public IMExExcelUsecase(
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
        }

        public void ExportTo(string saveTo)
        {
            var root = _loadedModel.MotionData!;
            var exchanger = new DataExchange();
            DoDataExchange(root, exchanger);
            using var stream = File.Create(saveTo);
            exchanger.SaveTo(stream);
        }

        private void DoDataExchange(Motion.InterpolatedMotion root, DataExchange exchanger)
        {
            exchanger.Sheet(
                "InitialPose",
                root.InitialPoses,
                sheetDef => sheetDef
                    .Column(nameof(Motion.InitialPose.BoneId), (row, cell) => cell.SetCellValue(row.BoneId), (row, cell) => row.BoneId = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.InitialPose.Channel), (row, cell) => cell.SetCellValue(row.Channel), (row, cell) => row.Channel = (short)cell.NumericCellValue)
                    .Column(nameof(Motion.InitialPose.Value), (row, cell) => cell.SetCellValue(row.Value), (row, cell) => row.Value = (float)cell.NumericCellValue)
            );
        }

        private class DataExchange
        {
            private XSSFWorkbook _book = new XSSFWorkbook();

            public void SaveTo(Stream stream)
            {
                _book.Write(stream);
            }

            public void Sheet<RowType>(
                string sheetName,
                List<RowType> list,
                Action<SheetDef<RowType>> onDefine
            )
                where RowType : new()
            {
                var xlsxSheet = _book.CreateSheet(sheetName);
                var sheetDef = new SheetDef<RowType>();
                onDefine(sheetDef);
                {
                    var xlsxHeader = xlsxSheet.CreateRow(0);
                    foreach (var (column, index) in sheetDef.Columns.SelectWithIndex())
                    {
                        var cell = xlsxHeader.CreateCell(index);
                        cell.SetCellValue(column.Header);
                    }
                }
                foreach (var (row, rowIndex) in list.SelectWithIndex())
                {
                    var xlsxData = xlsxSheet.CreateRow(1 + rowIndex);
                    foreach (var (column, cellIndex) in sheetDef.Columns.SelectWithIndex())
                    {
                        var cell = xlsxData.CreateCell(cellIndex);
                        column.Exporter(row!, cell);
                    }
                }
                xlsxSheet.CreateFreezePane(0, 1);
            }
        }

        private record ColumnDef(
            string Header,
            Action<object, ICell> Exporter,
            Action<object, ICell> Importer
        )
        {

        }

        private class SheetDef<RowType>
        {
            internal List<ColumnDef> Columns { get; set; } = new List<ColumnDef>();

            public SheetDef<RowType> Column(
                string header,
                Action<RowType, ICell> exporter,
                Action<RowType, ICell> importer
            )
            {
                Columns.Add(
                    new ColumnDef(
                        header,
                        (any, cell) => exporter((RowType)any, cell),
                        (any, cell) => importer((RowType)any, cell)
                    )
                );

                return this;
            }
        }
    }
}
