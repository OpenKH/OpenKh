using OpenKh.Tools.Kh2FontImageEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class CreateGlyphCellsUsecase
    {
        public GlyphCell[] CreateSimpleGlyphCells(int bitmapWidth, int bitmapHeight, int glyphWidth, int glyphHeight)
        {
            var list = new List<GlyphCell>();
            var idx = 0;

            for (int y = 0; y + glyphHeight <= bitmapHeight; y += glyphHeight)
            {
                for (int x = 0; x + glyphWidth <= bitmapWidth; x += glyphWidth, idx++)
                {
                    list.Add(new GlyphCell(new Rectangle(x, y, glyphWidth, glyphHeight), idx));
                }
            }

            return list.ToArray();
        }

        public GlyphCell[] CreateFontGlyphCells(int bitmapWidth, int bitmapHeight, int blockHeight, bool isFront, int glyphWidth, int glyphHeight)
        {
            var list = new List<GlyphCell>();

            var countPerBlock = (bitmapWidth / glyphWidth) * (blockHeight / glyphHeight);

            for (int blockY = 0, blockYIdx = 0; blockY + blockHeight <= bitmapHeight; blockY += blockHeight, blockYIdx++)
            {
                var blockYTo = blockY + blockHeight;

                var localIdx = countPerBlock * (2 * blockYIdx + (isFront ? 0 : 1));

                // F B
                // - -
                // 0 1
                // 2 3

                for (int y = blockY; y + glyphHeight <= blockYTo; y += glyphHeight)
                {
                    for (int x = 0; x + glyphWidth <= bitmapWidth; x += glyphWidth, localIdx++)
                    {
                        list.Add(new GlyphCell(new Rectangle(x, y, glyphWidth, glyphHeight), localIdx));
                    }
                }
            }

            return list.ToArray();
        }
    }
}
