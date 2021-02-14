using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Imaging
{
    public class PngImage : IImageRead
    {
        private byte[] _data;
        private byte[] _clut;

        internal enum ColorType
        {
            TrueColor = 2,
            Indexed = 3,
            AlphaTrueColor = 6,
        }

        internal enum ComprMethod
        {
            Deflate = 0,
        }

        internal enum InterlaceMethod
        {
            None = 0,
            Adam7 = 1,
        }

        internal class Signature
        {
            [Data] public ulong Magic { get; set; }

            public const ulong Valid = 727905341920923785;
        }

        internal class Chunk
        {
            [Data] public int Length { get; set; }
            [Data] public int CType { get; set; }
            public byte[] RawData { get; set; }
            public int Crc { get; set; }

            public const int IHDR = 0x49484452;
            public const int IDAT = 0x49444154;
            public const int IEND = 0x49454E44;
            public const int PLTE = 0x504C5445;
            public const int tRNS = 0x74524E53;
        }

        internal class IHdr
        {
            [Data] public int Width { get; set; }
            [Data] public int Height { get; set; }
            [Data] public byte Bits { get; set; }
            [Data] public byte ColorType { get; set; }
            [Data] public byte ComprMethod { get; set; }
            [Data] public byte FilterMethod { get; set; }
            [Data] public byte InterlaceMethod { get; set; }
        }

        public PngImage(Stream stream)
        {
            stream.SetPosition(0);
            var header = BinaryMapping.ReadObject<Signature>(stream);
            if (header.Magic != Signature.Valid)
            {
                throw new InvalidDataException("Bad signature code!");
            }

            var fullData = new MemoryStream();
            IHdr ihdr = null;
            byte[] PLTE = null;
            byte[] tRNS = null;

            while (stream.Position < stream.Length)
            {
                var chunk = BinaryMapping.ReadObject<Chunk>(stream);
                chunk.CType = Turn(chunk.CType);
                chunk.Length = Turn(chunk.Length);
                chunk.RawData = stream.ReadBytes(chunk.Length);
                chunk.Crc = Turn(stream.ReadInt32());

                if (chunk.CType == Chunk.IEND)
                {
                    break;
                }
                switch (chunk.CType)
                {
                    case Chunk.IHDR:
                        ihdr = BinaryMapping.ReadObject<IHdr>(new MemoryStream(chunk.RawData));
                        ihdr.Width = Turn(ihdr.Width);
                        ihdr.Height = Turn(ihdr.Height);
                        break;

                    case Chunk.PLTE:
                        PLTE = chunk.RawData;
                        break;

                    case Chunk.tRNS:
                        tRNS = chunk.RawData;
                        break;

                    case Chunk.IDAT:
                        fullData.Write(chunk.RawData);
                        break;
                }
            }

            if (ihdr == null)
            {
                throw new InvalidDataException("No IHDR!");
            }

            var interlaceMethod = (InterlaceMethod)ihdr.InterlaceMethod;

            if (interlaceMethod != InterlaceMethod.None)
            {
                throw new NotSupportedException($"interlaceMethod {interlaceMethod} not supported!");
            }

            var comprMethod = (ComprMethod)ihdr.ComprMethod;

            if (comprMethod != ComprMethod.Deflate)
            {
                throw new NotSupportedException($"comprMethod {comprMethod} not supported!");
            }

            fullData.Position = 2;

            var deflater = new DeflateStream(fullData, CompressionMode.Decompress);

            Size = new Size(ihdr.Width, ihdr.Height);

            var bits = ihdr.Bits;
            var colorType = (ColorType)ihdr.ColorType;

            if (bits == 4 && colorType == ColorType.Indexed)
            {
                PixelFormat = PixelFormat.Indexed4;
                var stride = (1 + Size.Width) / 2;
                _data = new byte[stride * Size.Height];
                for (int y = 0; y < Size.Height; y++)
                {
                    var filter = deflater.ReadByte();
                    deflater.Read(_data, y * stride, stride);
                    ApplyFilter(_data, y * stride, 1, stride, filter);
                }
                _clut = PrepareClut(PLTE, tRNS, 16);
            }
            else if (bits == 8 && colorType == ColorType.Indexed)
            {
                PixelFormat = PixelFormat.Indexed8;
                var stride = Size.Width;
                _data = new byte[stride * Size.Height];
                for (int y = 0; y < Size.Height; y++)
                {
                    var filter = deflater.ReadByte();
                    deflater.Read(_data, y * stride, stride);
                    ApplyFilter(_data, y * stride, 1, stride, filter);
                }
                _clut = PrepareClut(PLTE, tRNS, 256);
            }
            else if (bits == 8 && colorType == ColorType.TrueColor)
            {
                PixelFormat = PixelFormat.Rgb888;
                var stride = 3 * Size.Width;
                _data = new byte[stride * Size.Height];
                for (int y = 0; y < Size.Height; y++)
                {
                    var filter = deflater.ReadByte();
                    deflater.Read(_data, y * stride, stride);
                    ApplyFilter(_data, y * stride, 3, stride, filter);
                }
            }
            else if (bits == 8 && colorType == ColorType.AlphaTrueColor)
            {
                PixelFormat = PixelFormat.Rgba8888;
                var stride = 4 * Size.Width;
                _data = new byte[stride * Size.Height];
                for (int y = 0; y < Size.Height; y++)
                {
                    var filter = deflater.ReadByte();
                    deflater.Read(_data, y * stride, stride);
                    ApplyFilter(_data, y * stride, 4, stride, filter);
                }

                int localOfs = 0;

                for (int y = 0; y < Size.Height; y++)
                {
                    for (int x = 0; x < Size.Width; x++, localOfs += 4)
                    {
                        var r = _data[localOfs + 0];
                        var g = _data[localOfs + 1];
                        var b = _data[localOfs + 2];
                        var a = _data[localOfs + 3];

                        _data[localOfs + 0] = b;
                        _data[localOfs + 1] = g;
                        _data[localOfs + 2] = r;
                        _data[localOfs + 3] = a;
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Not supported combination: bits = {bits} and colorType = {colorType}");
            }
        }

        private void ApplyFilter(byte[] data, int ptr, int pixelSize, int stride, int filter)
        {
            // See: https://www.w3.org/TR/PNG-Filters.html
            // See: https://www.w3.org/TR/2003/REC-PNG-20031110/#9FtIntro

            if (filter == 0)
            {
                // nop
            }
            else if (filter == 1)
            {
                var endPtr = ptr + stride;
                ptr += pixelSize;
                for (; ptr < endPtr; ptr++)
                {
                    data[ptr] += data[ptr - pixelSize];
                }
            }
            else if (filter == 2)
            {
                var endPtr = ptr + stride;
                for (; ptr < endPtr; ptr++)
                {
                    data[ptr] += data[ptr - stride];
                }
            }
            else if (filter == 3)
            {
                var endPtr = ptr + stride;
                var atNextPixel = ptr + pixelSize;
                for (; ptr < atNextPixel; ptr++)
                {
                    data[ptr] += (byte)((0 + data[ptr - stride]) / 2);
                }
                for (; ptr < endPtr; ptr++)
                {
                    data[ptr] += (byte)((data[ptr - pixelSize] + data[ptr - stride]) / 2);
                }
            }
            else if (filter == 4)
            {
                var endPtr = ptr + stride;
                var atNextPixel = ptr + pixelSize;
                for (; ptr < atNextPixel; ptr++)
                {
                    data[ptr] += data[ptr - stride];
                }
                for (; ptr < endPtr; ptr++)
                {
                    data[ptr] += PaethPredictor(
                        data[ptr - pixelSize],
                        data[ptr - stride],
                        data[ptr - pixelSize - stride]
                    );
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private byte PaethPredictor(byte a, byte b, byte c)
        {
            var p = a + b - c;
            var pa = Math.Abs(p - a);
            var pb = Math.Abs(p - b);
            var pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc)
            {
                return (byte)a;
            }
            else if (pb <= pc)
            {
                return (byte)b;
            }
            else
            {
                return (byte)c;
            }
        }

        private byte[] PrepareClut(byte[] PLTE, byte[] tRNS, int count)
        {
            var clut = new byte[4 * count];
            for (int y = 0; y < count; y++)
            {
                if (3 * (y + 1) <= PLTE.Length)
                {
                    clut[4 * y + 0] = PLTE[3 * y + 0];
                    clut[4 * y + 1] = PLTE[3 * y + 1];
                    clut[4 * y + 2] = PLTE[3 * y + 2];
                }
                if (y + 1 <= tRNS.Length)
                {
                    clut[4 * y + 3] = tRNS[y];
                }
            }
            return clut;
        }

        private static int Turn(int val)
        {
            return (int)Turn((uint)val);
        }

        private static uint Turn(uint val)
        {
            return 0
                | ((val << 24))
                | ((val << 8) & 0x00FF0000)
                | ((val >> 8) & 0x0000FF00)
                | ((val >> 24) & 0x000000FF)
                ;
        }

        public static bool IsValid(Stream stream)
        {
            stream.SetPosition(0);
            return stream.ReadByte() == 0x89 &&
                stream.ReadByte() == 0x50 &&
                stream.ReadByte() == 0x4e &&
                stream.ReadByte() == 0x47 &&
                stream.ReadByte() == 0x0d &&
                stream.ReadByte() == 0x0a &&
                stream.ReadByte() == 0x1a &&
                stream.ReadByte() == 0x0a;
        }

        public static PngImage Read(Stream stream) => new PngImage(stream);

        #region IImageRead
        public Size Size { get; internal set; }

        public PixelFormat PixelFormat { get; internal set; }

        public byte[] GetData() => _data;

        public byte[] GetClut() => _clut;
        #endregion
    }
}
