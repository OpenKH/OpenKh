using kh.common;
using System.IO;

namespace kh.kh2
{
    public class Msg
    {
        private static uint MagicCode = 1;

        public static bool IsValid(Stream stream) =>
            new BinaryReader(stream).PeekUInt32() == MagicCode;
    }
}
