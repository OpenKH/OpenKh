using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Common
{
    public static class StreamExtensions
    {
        private static readonly InvalidDataException ReadSeekException = new InvalidDataException($"Stream must be readable and seekable.");
        private static readonly InvalidDataException WriteSeekException = new InvalidDataException($"Stream must be writable and seekable.");
        private static readonly Func<long, InvalidDataException> InvalidHeaderLengthException = (minHeaderLength) =>
            new InvalidDataException($"Invalid header: stream must be at least {minHeaderLength} bytes long.");

        public static T FromBegin<T>(this T stream) where T : Stream => stream.SetPosition(0);

        public static T SetPosition<T>(this T stream, long position) where T : Stream
        {
            stream.Seek(position, SeekOrigin.Begin);
            return stream;
        }

        public static T MustReadAndSeek<T>(this T stream) where T : Stream
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw ReadSeekException;
            return stream;
        }

        public static T MustWriteAndSeek<T>(this T stream) where T : Stream
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw WriteSeekException;
            return stream;
        }

        public static T MustHaveHeaderLengthOf<T>(this T stream, long minHeaderLength) where T : Stream
        {
            if (stream.Length < minHeaderLength)
                throw InvalidHeaderLengthException(minHeaderLength);
            return stream;
        }

        public static T AlignPosition<T>(this T stream, int alignValue) where T : Stream =>
            stream.SetPosition(Helpers.Align(stream.Position, alignValue));

        public static List<T> ReadList<T>(this Stream stream, int offset, int count)
            where T : class
        {
            stream.Position = offset;
            return stream.ReadList<T>(count);
        }

        public static List<T> ReadList<T>(this Stream stream, int count)
            where T : class
        {
            return Enumerable.Range(0, count)
                .Select(x => BinaryMapping.ReadObject<T>(stream, (int)stream.Position))
                .ToList();
        }

        unsafe public static short ReadInt16(this Stream stream)
        {
            var buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            fixed (byte* ptr = buffer)
                return *(short*)ptr;
        }

        unsafe public static ushort ReadUInt16(this Stream stream)
        {
            var buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            fixed (byte* ptr = buffer)
                return *(ushort*)ptr;
        }

        unsafe public static int ReadInt32(this Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            fixed (byte* ptr = buffer)
                return *(int*)ptr;
        }

        unsafe public static uint ReadUInt32(this Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            fixed (byte* ptr = buffer)
                return *(uint*)ptr;
        }

        unsafe public static long ReadInt64(this Stream stream)
        {
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            fixed (byte* ptr = buffer)
                return *(long*)ptr;
        }

        unsafe public static ulong ReadUInt64(this Stream stream)
        {
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            fixed (byte* ptr = buffer)
                return *(ulong*)ptr;
        }

        unsafe public static float ReadSingle(this Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            fixed (byte* ptr = buffer)
                return *(float*)ptr;
        }

        unsafe public static float ReadFloat(this Stream stream) => stream.ReadSingle();

        unsafe public static double ReadDouble(this Stream stream)
        {
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            fixed (byte* ptr = buffer)
                return *(double*)ptr;
        }

        unsafe public static Vector2 ReadVector2(this Stream stream)
        {
            var buffer = new byte[2 * sizeof(float)];
            stream.Read(buffer, 0, 2 * sizeof(float));
            fixed (byte* ptr = buffer)
                return *(Vector2*)ptr;
        }

        unsafe public static Vector3 ReadVector3(this Stream stream)
        {
            var buffer = new byte[3 * sizeof(float)];
            stream.Read(buffer, 0, 3 * sizeof(float));
            fixed (byte* ptr = buffer)
                return *(Vector3*)ptr;
        }

        unsafe public static Vector4 ReadVector4(this Stream stream)
        {
            var buffer = new byte[4 * sizeof(float)];
            stream.Read(buffer, 0, 4 * sizeof(float));
            fixed (byte* ptr = buffer)
                return *(Vector4*)ptr;
        }

        unsafe public static Matrix4x4 ReadMatrix4x4(this Stream stream)
        {
            var buffer = new byte[4 * 4 * sizeof(float)];
            stream.Read(buffer, 0, 4 * 4 * sizeof(float));
            fixed (byte* ptr = buffer)
                return *(Matrix4x4*)ptr;
        }

        public static List<int> ReadInt32List(this Stream stream, int offset, int count)
        {
            stream.Position = offset;
            return stream.ReadInt32List(count);
        }

        public static List<int> ReadInt32List(this Stream stream, int count)
        {
            var reader = new BinaryReader(stream);
            return Enumerable.Range(0, count)
                .Select(x => reader.ReadInt32())
                .ToList();
        }

        public static byte[] ReadBytes(this Stream stream) =>
            stream.ReadBytes((int)(stream.Length - stream.Position));

        public static byte[] ReadBytes(this Stream stream, int length)
        {
            var data = new byte[length];
            stream.Read(data, 0, length);
            return data;
        }

        public static byte[] ReadAllBytes(this Stream stream)
        {
            var data = stream.SetPosition(0).ReadBytes();
            stream.Position = 0;
            return data;
        }

        public static string ReadString(this Stream stream, int maxLength, Encoding encoding)
        {
            var data = stream.ReadBytes(maxLength);
            var terminatorIndex = Array.FindIndex(data, x => x == 0);
            return encoding.GetString(data, 0, terminatorIndex < 0 ? maxLength : terminatorIndex);
        }

        public static byte PeekByte(this Stream stream) => stream.Peek(x => (byte)x.ReadByte());
        public static short PeekInt16(this Stream stream) => stream.Peek(x => x.ReadInt16());
        public static ushort PeekUInt16(this Stream stream) => stream.Peek(x => x.ReadUInt16());
        public static int PeekInt32(this Stream stream) => stream.Peek(x => x.ReadInt32());
        public static uint PeekUInt32(this Stream stream) => stream.Peek(x => x.ReadUInt32());
        public static long PeekInt64(this Stream stream) => stream.Peek(x => x.ReadInt64());
        public static ulong PeekUInt64(this Stream stream) => stream.Peek(x => x.ReadUInt64());

        public static T Peek<T>(this Stream stream, Func<Stream, T> func)
        {
            var currentPosition = stream.Position;
            var result = func(stream);
            stream.SetPosition(currentPosition);
            return result;
        }

        public static int WriteList<T>(this Stream stream, IEnumerable<T> items)
            where T : class
        {
            var oldPosition = (int)stream.Position;
            foreach (var item in items)
                BinaryMapping.WriteObject<T>(stream, item, oldPosition);

            return (int)stream.Position - oldPosition;
        }

        public static void Write(this Stream stream, byte[] data) =>
            stream.Write(data, 0, data.Length);

        public static int Write(this Stream stream, IEnumerable<int> items)
        {
            var oldPosition = (int)stream.Position;
            var writer = new BinaryWriter(stream);
            foreach (var item in items)
                writer.Write(item);

            return (int)stream.Position - oldPosition;
        }

        unsafe public static void Write(this Stream stream, byte value) => stream.WriteByte(value);
        unsafe public static void Write(this Stream stream, sbyte value) => stream.WriteByte((byte)value);
        unsafe public static void Write(this Stream stream, char value) => stream.WriteByte((byte)value);
        unsafe public static void Write(this Stream stream, short value)
        {
            var buffer = new byte[2];
            fixed (byte* ptr = buffer)
                *(short*)ptr = value;
            stream.Write(buffer, 0, 2);
        }
        unsafe public static void Write(this Stream stream, ushort value)
        {
            var buffer = new byte[2];
            fixed (byte* ptr = buffer)
                *(ushort*)ptr = value;
            stream.Write(buffer, 0, 2);
        }
        unsafe public static void Write(this Stream stream, int value)
        {
            var buffer = new byte[4];
            fixed (byte* ptr = buffer)
                *(int*)ptr = value;
            stream.Write(buffer, 0, 4);
        }
        unsafe public static void Write(this Stream stream, uint value)
        {
            var buffer = new byte[4];
            fixed (byte* ptr = buffer)
                *(uint*)ptr = value;
            stream.Write(buffer, 0, 4);
        }
        unsafe public static void Write(this Stream stream, long value)
        {
            var buffer = new byte[8];
            fixed (byte* ptr = buffer)
                *(long*)ptr = value;
            stream.Write(buffer, 0, 8);
        }
        unsafe public static void Write(this Stream stream, ulong value)
        {
            var buffer = new byte[8];
            fixed (byte* ptr = buffer)
                *(ulong*)ptr = value;
            stream.Write(buffer, 0, 8);
        }
        unsafe public static void Write(this Stream stream, float value)
        {
            var buffer = new byte[4];
            fixed (byte* ptr = buffer)
                *(float*)ptr = value;
            stream.Write(buffer, 0, 4);
        }
        unsafe public static void Write(this Stream stream, Vector2 value)
        {
            var buffer = new byte[2 * sizeof(float)];
            fixed (byte* ptr = buffer)
                *(Vector2*)ptr = value;
            stream.Write(buffer, 0, 2 * sizeof(float));
        }
        unsafe public static void Write(this Stream stream, Vector3 value)
        {
            var buffer = new byte[3 * sizeof(float)];
            fixed (byte* ptr = buffer)
                *(Vector3*)ptr = value;
            stream.Write(buffer, 0, 3 * sizeof(float));
        }
        unsafe public static void Write(this Stream stream, Vector4 value)
        {
            var buffer = new byte[4 * sizeof(float)];
            fixed (byte* ptr = buffer)
                *(Vector4*)ptr = value;
            stream.Write(buffer, 0, 4 * sizeof(float));
        }
        unsafe public static void Write(this Stream stream, Matrix4x4 value)
        {
            var buffer = new byte[4 * 4 * sizeof(float)];
            fixed (byte* ptr = buffer)
                *(Matrix4x4*)ptr = value;
            stream.Write(buffer, 0, 4 * 4 * sizeof(float));
        }

        public static void Copy(this Stream source, Stream destination, int length, int bufferSize = 65536)
        {
            int read;
            byte[] buffer = new byte[Math.Min(length, bufferSize)];

            while ((read = source.Read(buffer, 0, Math.Min(length, bufferSize))) != 0)
            {
                destination.Write(buffer, 0, read);
                length -= read;
            }
        }
    }
}
