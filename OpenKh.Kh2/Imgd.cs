using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2
{
    public partial class Imgd : IImageRead
	{
		private const uint MagicCode = 0x44474D49U;
        private const short Format32bpp = 0x00;
        private const short Format8bpp = 0x13;
        private const short Format4bpp = 0x14;
        private const short SubFormat32bpp = 3;
        private const short SubFormat8bpp = 5;
        private const short SubFormat4bpp = 4;

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 && new BinaryReader(stream).PeekInt32() == MagicCode;

        private readonly short format;
        private readonly int swizzled;

		private Imgd(Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new InvalidDataException($"Read or seek must be supported.");

			var reader = new BinaryReader(stream);
			if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
				throw new InvalidDataException("Invalid header");

			var unk04 = reader.ReadInt32();
			var dataOffset = reader.ReadInt32();
			var dataLength = reader.ReadInt32();
			var palOffset = reader.ReadInt32();
			var palLength = reader.ReadInt32();
			var unk18 = reader.ReadInt32();
			var width = reader.ReadInt16();
			var height = reader.ReadInt16();
			var powWidth = reader.ReadInt16();
			var powHeight = reader.ReadInt16();
			var widthDiv64 = reader.ReadInt16();
			format = reader.ReadInt16();
			var unk28 = reader.ReadInt32();
			var unk2c = reader.ReadInt16();
			var unk2e = reader.ReadInt16();
			var unk30 = reader.ReadInt32();
            var unk34 = reader.ReadInt16();
			var unk36 = reader.ReadInt16();
			var unk38 = reader.ReadInt32();
			swizzled = reader.ReadInt32();

			Size = new Size(width, height);
			Data = reader.ReadBytes(dataLength);
			Clut = reader.ReadBytes(palLength);
        }

        public static Imgd Read(Stream stream) => new Imgd(stream);

        public void Write(Stream stream)
		{
			if (!stream.CanWrite || !stream.CanSeek)
				throw new InvalidDataException($"Write or seek must be supported.");

			var writer = new BinaryWriter(stream);
			var baseOffset = (int)stream.Position;

			var dataOffset = baseOffset + 0x40;
			var palOffset = dataOffset + Data.Length;

			writer.Write(MagicCode);
			writer.Write(256);
			writer.Write(dataOffset);
			writer.Write(Data.Length);
			writer.Write(palOffset);
			writer.Write(Clut.Length);
			writer.Write(-1);
			writer.Write((short)Size.Width);
			writer.Write((short)Size.Height);
			writer.Write(GetPow((short)Size.Width));
			writer.Write(GetPow((short)Size.Height));
			writer.Write((short)(Size.Width / 64));
			writer.Write(format);
			writer.Write(-1);
			writer.Write((short)(format == Format4bpp ? 8 : 16));
            writer.Write((short)(format == Format4bpp ? 2 : 16));
			writer.Write((short)1);
			writer.Write((short)(format == Format32bpp ? 19 : 0));
			writer.Write(GetSubFormat(format));
			writer.Write((short)(format == Format32bpp ? 0 : 3));
            writer.Write(0);

			writer.Write(swizzled);

            writer.Write(Data, 0, Data.Length);
			writer.Write(Clut, 0, Clut.Length);
		}

		public Size Size { get; }

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
                    throw new NotSupportedException($"The format {format} is not supported.");
            }
        }

        private byte[] GetClut4()
        {
            var data = new byte[16 * 4];
            for (var i = 0; i < 16; i++)
            {
                data[i * 4 + 0] = Clut[(i & 15) * 4 + 0];
                data[i * 4 + 1] = Clut[(i & 15) * 4 + 1];
                data[i * 4 + 2] = Clut[(i & 15) * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(Clut[(i & 15) * 4 + 3]);
            }

            return data;
        }

        private byte[] GetClut8()
        {
            var data = new byte[256 * 4];
            for (var i = 0; i < 256; i++)
            {
                var srcIndex = Ps2.Repl(i);
                data[i * 4 + 0] = Clut[srcIndex * 4 + 0];
                data[i * 4 + 1] = Clut[srcIndex * 4 + 1];
                data[i * 4 + 2] = Clut[srcIndex * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(Clut[srcIndex * 4 + 3]);
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

        private static short GetPow(short value)
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
