using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Command.ImgTool.Interfaces
{
    interface ICommonQuantizerParam
    {
        public int BitsPerPixel { get; }

        public bool PngQuant { get; }
    }
}
