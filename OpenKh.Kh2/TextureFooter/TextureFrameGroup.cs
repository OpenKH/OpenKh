using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2.TextureFooter
{
    public class TextureFrameGroup
    {
        /// <summary>
        /// Using Dictionary instead of List so that we can capture all of frames which are using relative jumps.
        /// </summary>
        public IDictionary<int, TextureFrame> IndexedFrameList { get; set; }

        public override string ToString() => $"{IndexedFrameList.Count} frames";
    }

}
