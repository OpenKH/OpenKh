using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Xe.BinaryMapper;
using Xe.IO;

namespace OpenKh.Kh2
{
    public partial class Imgd : IImageRead
	{
        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int BitmapOffset { get; set; }
            [Data] public int BitmapLength { get; set; }
            [Data] public int ClutOffset { get; set; }
            [Data] public int ClutLength { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public short Width { get; set; }
            [Data] public short Height { get; set; }
            [Data] public short PowWidth { get; set; }
            [Data] public short PowHeight { get; set; }
            [Data] public short WidthDiv64 { get; set; }
            [Data] public short Format { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public short Unk2c { get; set; }
            [Data] public short Unk2e { get; set; }
            [Data] public short Unk30 { get; set; }
            [Data] public short Unk32 { get; set; }
            [Data] public short Unk34 { get; set; }
            [Data] public short Unk36 { get; set; }
            [Data] public int Unk38 { get; set; }
            [Data] public int Swizzled { get; set; }
        }

		private const uint MagicCode = 0x44474D49U;
        private const int HeaderLength = 0x40;
        private const short Format32bpp = 0x00;
        private const short Format8bpp = 0x13;
        private const short Format4bpp = 0x14;
        private const short SubFormat32bpp = 3;
        private const short SubFormat8bpp = 5;
        private const short SubFormat4bpp = 4;
        private const int FacAlignment = 0x800;
        private static readonly InvalidDataException InvalidHeaderException = new InvalidDataException("Invalid header");

        public static bool IsValid(Stream stream) =>
            stream.Length >= HeaderLength && stream.SetPosition(0).ReadInt32() == MagicCode;

        private readonly short format;
        private readonly int swizzled;

		private Imgd(Stream stream)
		{
            stream
                .MustReadAndSeek()
                .MustHaveHeaderLengthOf(HeaderLength);

            var reader = new BinaryReader(stream);
            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCode)
                throw InvalidHeaderException;

            Size = new Size(header.Width, header.Height);
            format = header.Format;
            swizzled = header.Swizzled;

            stream.SetPosition(header.BitmapOffset);
            var data = reader.ReadBytes(header.BitmapLength);

            // Swap pixel order for only unswizzled 4-bpp IMGD.
            Data = (format == Format4bpp && (swizzled & 4) == 0)
                ? GetSwappedPixelData(data)
                : data;

            stream.SetPosition(header.ClutOffset);
            Clut = reader.ReadBytes(header.ClutLength);
        }

        public static Imgd Read(Stream stream) => new Imgd(stream.SetPosition(0));

        public void Write(Stream stream)
		{
            stream.MustWriteAndSeek();

            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Unk04 = 0x100,
                BitmapOffset = HeaderLength,
                BitmapLength = Data.Length,
                ClutOffset = HeaderLength + Data.Length,
                ClutLength = Clut?.Length ?? 0,
                Unk18 = -1,
                Width = (short)Size.Width,
                Height = (short)Size.Height,
                PowWidth = GetPow(Size.Width),
                PowHeight = GetPow(Size.Height),
                WidthDiv64 = (short)(Size.Width / 64),
                Format = format,
                Unk28 = -1,
                Unk2c = (short)(format == Format4bpp ? 8 : 16),
                Unk2e = (short)(format == Format4bpp ? 2 : 16),
                Unk30 = 1,
                Unk32 = (short)(format == Format32bpp ? 19 : 0),
                Unk34 = GetSubFormat(format),
                Unk36 = (short)(format == Format32bpp ? 0 : 3),
                Unk38 = 0,
                Swizzled = swizzled,
            });

            // Swap pixel order for only unswizzled 4-bpp IMGD.
            var data = (format == Format4bpp && (swizzled & 4) == 0)
                ? GetSwappedPixelData(Data)
                : Data;

            stream.Write(data, 0, data.Length);

            if (Clut != null)
                stream.Write(Clut, 0, Clut.Length);
        }

        public static bool IsFac(Stream stream)
        {
            if (stream.Length < HeaderLength)
                return false;

            stream.MustReadAndSeek().SetPosition(0);
            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCode)
                return false;

            stream
                .SetPosition(header.ClutOffset + header.ClutLength)
                .AlignPosition(FacAlignment);

            if (stream.Position + HeaderLength >= stream.Length)
                return false;

            header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCode)
                return false;

            return true;
        }

        public static IEnumerable<Imgd> ReadAsFac(Stream stream)
        {
            stream.SetPosition(0);
            while (true)
            {
                stream.AlignPosition(FacAlignment);
                var subStreamLength = stream.Length - stream.Position;
                if (subStreamLength < HeaderLength)
                    yield break;

                yield return Imgd.Read(new SubStream(stream, stream.Position, subStreamLength));
            }
        }

        public static void WriteAsFac(Stream stream, IEnumerable<Imgd> images)
        {
            foreach (var image in images)
            {
                image.Write(stream);
                stream.SetLength(stream.AlignPosition(FacAlignment).Position);
            }
        }

        public Size Size { get; }

        /// <summary>
        /// Bitmap data
        /// </summary>
        /// <remarks>
        /// In the following case, this `Data` is not same bitmap data stored in imgd file.
        /// 
        /// In OpenKh:
        /// - NOT IsSwizzled && Format4bpp = storing Windows pixel order ([1, 2] to 0x12)
        /// 
        /// In IMGD file:
        /// - NOT IsSwizzled && Format4bpp = storing reversed pixel order ([1, 2] to 0x21)
        /// </remarks>
		public byte[] Data { get; }

        public byte[] Clut { get; }

        public bool IsSwizzled => (swizzled & 4) != 0;

        public PixelFormat PixelFormat => GetPixelFormat(format);

        public byte[] GetData()
		{
			switch (format)
            {
                case Format32bpp:
                    return GetData32bpp();
                case Format8bpp:
                    return IsSwizzled ? Ps2.Decode8(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 64), Size.Width / 128, Size.Height / 64) : Data;
                case Format4bpp:
					return IsSwizzled ? Ps2.Decode4(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 128), Size.Width / 128, Size.Height / 128) : Data;
				default:
					throw new NotSupportedException($"The format {format} is not supported.");
			}
        }

        public byte[] GetClut()
        {
            switch (format)
            {
                case Format8bpp: return GetClut8();
                case Format4bpp: return GetClut4();
                default:
                    throw new NotSupportedException($"The format {format} is not supported or does not contain any palette.");
            }
        }

        private byte[] GetClut4()
        {
            var data = new byte[16 * 4];
            for (var i = 0; i < 16; i++)
            {
                data[i * 4 + 0] = Clut[i * 4 + 0];
                data[i * 4 + 1] = Clut[i * 4 + 1];
                data[i * 4 + 2] = Clut[i * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(Clut[i * 4 + 3]);
            }

            return data;
        }

        private byte[] GetClut8()
        {
            var data = new byte[256 * 4];
            for (var i = 0; i < 256; i++)
            {
                var srcIndex = Ps2.Repl(i);
                if (srcIndex * 4 < Clut.Length)
                {
                    data[i * 4 + 0] = Clut[srcIndex * 4 + 0];
                    data[i * 4 + 1] = Clut[srcIndex * 4 + 1];
                    data[i * 4 + 2] = Clut[srcIndex * 4 + 2];
                    data[i * 4 + 3] = Ps2.FromPs2Alpha(Clut[srcIndex * 4 + 3]);
                }
            }

            return data;
        }

        private byte[] GetData32bpp()
        {
            var newData = new byte[Data.Length];
            for (var i = 0; i < newData.Length - 3; i += 4)
            {
                newData[i + 0] = Data[i + 2];
                newData[i + 1] = Data[i + 1];
                newData[i + 2] = Data[i + 0];
                newData[i + 3] = Ps2.FromPs2Alpha(Data[i + 3]);
            }

            return newData;
        }

        private static short GetPow(int value)
        {
            short pow = 1;
            while (value > (1 << pow))
                pow++;

            return pow;
        }

        private static PixelFormat GetPixelFormat(int format)
        {
            switch (format)
            {
                case Format32bpp: return PixelFormat.Rgba8888;
                case Format8bpp: return PixelFormat.Indexed8;
                case Format4bpp: return PixelFormat.Indexed4;
                default: return PixelFormat.Undefined;
            }
        }

        private static short GetFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Rgba8888: return Format32bpp;
                case PixelFormat.Indexed4: return Format4bpp;
                case PixelFormat.Indexed8: return Format8bpp;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Pixel format {pixelFormat} is not supported.");
            }
        }

        private static short GetSubFormat(short format)
        {
            switch (format)
            {
                case Format32bpp: return SubFormat32bpp;
                case Format4bpp: return SubFormat4bpp;
                case Format8bpp: return SubFormat8bpp;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Format {format} is not supported.");
            }
        }
    }
}
