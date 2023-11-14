using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class LayoutOnMultiColumnsUsecase
    {
        public MultiColumnsLayouted Layout(
            Vector2 windowSize,
            float columnWidth,
            float rowHeight,
            int numItems
        )
        {
            int cx = (int)Math.Floor(windowSize.X / Math.Max(1, columnWidth));
            int cy = (int)((numItems + cx - 1) / Math.Max(1, cx));

            var cells = new List<MultiColumnsLayouted.Cell>();

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    var index = cy * x + y;
                    if (index < numItems)
                    {
                        cells.Add(new MultiColumnsLayouted.Cell(x, y, index));
                    }
                }
            }

            return new MultiColumnsLayouted(cx, cy, cells);
        }
    }
}
