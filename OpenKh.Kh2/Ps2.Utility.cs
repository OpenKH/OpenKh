using System;

namespace OpenKh.Kh2
{
    public partial class Ps2
    {
        public static byte FromPs2Alpha(byte alpha) =>
            (byte)Math.Min(alpha * 2, byte.MaxValue);
    }
}
