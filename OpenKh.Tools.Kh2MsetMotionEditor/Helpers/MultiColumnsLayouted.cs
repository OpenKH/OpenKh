using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
