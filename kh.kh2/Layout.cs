using kh.common;
using System.IO;

namespace kh.kh2
{
    public class Layout
    {
        private const uint MagicCodeValidator = 0x4459414CU;


        public static bool IsValid(Stream stream) =>
            stream.Length >= 32 && new BinaryReader(stream).PeekInt32() == MagicCodeValidator;
    }
}
