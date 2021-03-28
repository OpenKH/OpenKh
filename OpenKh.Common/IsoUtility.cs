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
            isoStream.SetPosition(basePosition);
            for (int i = 0; i < 0x500; i++)
            {
                if (isoStream.ReadByte() != 1)
                    continue;

                var stringLength = isoStream.ReadByte();
                if (stringLength != fileNameData.Length)
                    continue;

                var currentPosition = isoStream.Position;
                if (isoStream.ReadBytes(stringLength).SequenceEqual(fileNameData))
                {
                    isoStream.SetPosition(currentPosition - 0x1B);

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

                isoStream.Position = currentPosition + 1;
            }

            return -1;
        }
    }
}
