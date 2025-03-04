using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public record MultiColumnsLayouted(
        int NumColumns,
        int NumRows,
        IEnumerable<MultiColumnsLayouted.Cell> Cells
    )
    {
        public record Cell(int X, int Y, int Index)
        {

        }
    }
}
