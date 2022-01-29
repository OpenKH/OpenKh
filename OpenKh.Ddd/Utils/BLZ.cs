using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe;

namespace OpenKh.Ddd.Utils
{
    /*
     * BLZ - Backwards/Bottom LZ 
     * Also known as LZovl (Overlay LZ)
     * 
     * This is the encoding that is used with DS overlays & in some games like
     * 'RPG Maker DS'. Its main feature is that it can have an unencoded part before
     * the encoded part, which is a normal LZ file that starts from the end of the file
     * at the beginning.
     * 
     * It is read backwards
     * 
     * Special Thanks to "CUE"
     */
    public static class BLZ
    {
        public static byte[] Uncompress(Stream stream, int fileLength, bool printWarnings = false)
        {
            if (fileLength < 8)
                //TODO: better exception type(s) throughout this function.
                throw new Exception("Bad File Length!");

            byte[] packedBytes = new byte[fileLength];
            if (stream.Read(packedBytes) < fileLength)
            {
                throw new Exception("Unexpected end of file!");
            }

            var memoryStream = new MemoryStream(packedBytes);
            var binaryReader = new BinaryReader(memoryStream, Encoding.ASCII, true);

            memoryStream.Seek(-4, SeekOrigin.End);
            int extraBytes = binaryReader.ReadInt32();
            if (extraBytes == 0)
            {
                // No compression, copy in to out, minus last 4 bytes
                byte[] outBuffer = new byte[fileLength - 4];
                Buffer.BlockCopy(packedBytes, 0, outBuffer, 0, fileLength - 4);
                return outBuffer;
            }

            memoryStream.Seek(-5, SeekOrigin.End);
            int headerLength = (int)binaryReader.ReadByte();
            memoryStream.Seek(-8, SeekOrigin.End);
            int encodedLength = binaryReader.ReadInt32() & 0x00FFFFFF;
            int keepLength = fileLength - encodedLength;
            int dataLength = encodedLength - headerLength;
            int unpackedLength = keepLength + encodedLength + extraBytes;

            byte[] unpackedBytes = new byte[unpackedLength];

            Buffer.BlockCopy(packedBytes, 0, unpackedBytes, 0, keepLength);

            byte[] packedData = new byte[dataLength];
            byte[] decompressedData = new byte[dataLength + extraBytes + headerLength];

            Buffer.BlockCopy(packedBytes, keepLength, packedData, 0, dataLength);
            Array.Reverse(packedData);
            var packedReader = new BinaryReader(new MemoryStream(packedData), Encoding.ASCII, true);

            memoryStream.Seek(-headerLength, SeekOrigin.End);
            byte mask = 0, flags = 0;
            int bytesToWrite = unpackedLength - keepLength;
            int bytesRead = 0, bytesWritten = 0;
            while (bytesWritten < bytesToWrite)
            {
                mask >>= 1;
                if (mask == 0)
                {
                    if (bytesRead >= encodedLength)
                    {
                        throw new Exception("Unexpected end of data while reading flag byte!");
                    }
                    flags = packedReader.ReadByte();
                    bytesRead++;
                    mask = 0x80;
                }

                if ((flags & mask) > 0)
                {
                    if (bytesRead + 1 >= encodedLength)
                    {
                        throw new Exception("Unexpected end of data while reading decompression token!");
                    }

                    ushort info = (ushort)((packedReader.ReadByte() << 8) | packedReader.ReadByte());
                    bytesRead += 2;

                    var length = (info >> 12) + 3;
                    if (bytesWritten + length > bytesToWrite)
                    {
                        if (printWarnings)
                        {
                            Console.WriteLine("WARN: Final decompression token longer than remaining bytes to be written. Output may be truncated.");
                        }
                        length = bytesToWrite - bytesWritten;
                    }
                    var displacement = (info & 0xFFF) + 3;
                    Buffer.BlockCopy(decompressedData, bytesWritten - displacement, decompressedData, bytesWritten, length);
                    bytesWritten += length;
                }
                else
                {
                    if (bytesRead == encodedLength)
                    {
                        throw new Exception("Unexpected end of data while reading literal byte!");
                    }

                    byte b = packedReader.ReadByte();
                    bytesRead++;
                    decompressedData[bytesWritten++] = b;
                }
            }

            Array.Reverse(decompressedData);
            Buffer.BlockCopy(decompressedData, 0, unpackedBytes, keepLength, bytesToWrite);

            return unpackedBytes;
        }
    }
}
