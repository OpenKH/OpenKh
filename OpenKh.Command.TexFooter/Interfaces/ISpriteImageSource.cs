using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OpenKh.Command.TexFooter.Interfaces
{
    public interface ISpriteImageSource
    {
        /// <summary>
        /// Only 4 or 8
        /// </summary>
        int BitsPerPixel { get; }

        /// <summary>
        /// Width is 32, 64, or such.
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// For 4bpp: pass swapped data. 0x12 0x34 is [2, 1, 4, 3]
        /// For 8bpp: pass platform native data. 0x12 0x34 is [0x12, 0x34]
        /// </summary>
        byte[] Data { get; }
    }
}
