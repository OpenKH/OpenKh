using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Common
{
    public static class IsoUtility
    {
        private const int BlockLength = 0x800;

        public static int GetFileOffset(Stream isoStream, string fileName)
        {
            const uint basePosition = 0x105 * BlockLength; // un-needly hardcoded?

            var fileNameData = Encoding.UTF8.GetBytes(fileName);
            var needle = new byte[fileNameData.Length + 2];
            needle[0] = 0x01;
            needle[1] = 0x09;
            Array.Copy(fileNameData, 0, needle, 2, fileNameData.Length);

            for (int i = 0; i < 0x500; i++)
            {
                isoStream.Position = basePosition + i;
                var hayRead = isoStream.ReadBytes(needle.Length);
                if (hayRead.SequenceEqual(needle))
                {
                    isoStream.Position -= 0x24;

                    var blockStack = isoStream.ReadBytes(0x04);
                    var blockCorrectEndian = new byte[0x04]
                    {
                        blockStack[3],
                        blockStack[2],
                        blockStack[1],
                        blockStack[0]
                    };

                    return BitConverter.ToInt32(blockCorrectEndian, 0);
                }
            }

            return -1;
        }
    }
}
