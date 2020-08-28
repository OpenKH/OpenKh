using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Command.ImgTool.Interfaces
{
    public interface ICommonQuantizerParam
    {
        /// <summary>
        /// 4 or 8
        /// </summary>
        public int BitsPerPixel { get; }

        /// <summary>
        /// Invoke pngquant.exe, if true
        /// </summary>
        public bool PngQuant { get; }
    }
}
