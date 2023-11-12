using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Kh2.Utils
{
    public class ReadWriteUtils
    {
        public static void alignStreamToByte(Stream stream, int alignByte)
        {
            if (stream.Position % alignByte != 0)
            {
                byte[] extraBytes = new byte[(alignByte - (stream.Position % alignByte))];
                MemoryStream extraStream = new MemoryStream(extraBytes);
                extraStream.CopyTo(stream);
            }
        }
        public static void alignStreamToByte(Stream stream, int alignByte, byte padByte)
        {
            if (stream.Position % alignByte != 0)
            {
                BinaryWriter writer = new BinaryWriter(stream);
                int paddingSize = (int)(alignByte - (stream.Position % alignByte));
                for (int i = 0; i < paddingSize; i++)
                {
                    writer.Write(padByte);
                }
            }
        }
        public static void addBytesToStream(Stream stream, int count, byte padByte)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < count; i++)
            {
                writer.Write(padByte);
            }
        }
        public static int bytesRequiredToAlignToByte(long currentPosition, int alignByte)
        {
            return (int)(alignByte - (currentPosition % alignByte));
        }
    }
}
