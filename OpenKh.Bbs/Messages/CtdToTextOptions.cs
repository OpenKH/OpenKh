using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Bbs.Messages
{
    public class CtdToTextOptions
    {
        /// <summary>
        /// Allow to decode unknown as `{:unk FF}` alternative instead of throwing an exception.
        /// </summary>
        public bool AllowUnkUsage { get; set; } = true;

        /// <summary>
        /// Decode input as Shift_JIS encoding.
        /// </summary>
        public bool DecodeAsShiftJIS { get; set; }
    }
}
