using System;
using System.IO;

namespace OpenKh.Common
{
    public static class BinaryReaderHelpers
    {
        public static byte PeekByte(this BinaryReader reader) => reader.Peek(x => x.ReadByte());
        public static char PeekChar(this BinaryReader reader) => reader.Peek(x => x.ReadChar());
        public static short PeekInt16(this BinaryReader reader) => reader.Peek(x => x.ReadInt16());
        public static ushort PeekUInt16(this BinaryReader reader) => reader.Peek(x => x.ReadUInt16());
        public static int PeekInt32(this BinaryReader reader) => reader.Peek(x => x.ReadInt32());
        public static uint PeekUInt32(this BinaryReader reader) => reader.Peek(x => x.ReadUInt32());
        public static long PeekInt64(this BinaryReader reader) => reader.Peek(x => x.ReadInt64());
        public static ulong PeekUInt64(this BinaryReader reader) => reader.Peek(x => x.ReadUInt64());

        public static T Peek<T>(this BinaryReader reader, Func<BinaryReader, T> func)
        {
            var currentPosition = reader.BaseStream.Position;
            var result = func(reader);
            reader.BaseStream.Position = currentPosition;
            return result;
        }
    }
}
