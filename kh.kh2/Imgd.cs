using System;
using System.Drawing;
using System.IO;

namespace kh.kh2
{
    public partial class Imgd : IImage
	{
		private const uint MagicCode = 0x44474D49U;
		private int unk04 = 0x100;
		private int unk18 = -1;
		private short unk20 = 9;
		private short unk22 = 9;
		private short unk24 = 8;
		private short format;
		private int unk28 = -1;
		private short unk2c;
		private short unk2e;
		private int unk30;
		private short unk34;
		private short unk36;
		private int unk38;
		private int swizzled;

		public Imgd(Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new InvalidDataException($"Read or seek must be supported.");

			var reader = new BinaryReader(stream);
			if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
				throw new InvalidDataException("Invalid header");

			unk04 = reader.ReadInt32();
			var dataOffset = reader.ReadInt32();
			var dataLength = reader.ReadInt32();
			var palOffset = reader.ReadInt32();
			var palLength = reader.ReadInt32();
			unk18 = reader.ReadInt32();
			var width = reader.ReadInt16();
			var height = reader.ReadInt16();
			unk20 = reader.ReadInt16();
			unk22 = reader.ReadInt16();
			unk24 = reader.ReadInt16();
			format = reader.ReadInt16();
			unk28 = reader.ReadInt32();
			unk2c = reader.ReadInt16();
			unk2e = reader.ReadInt16();
			unk30 = reader.ReadInt32();
			unk34 = reader.ReadInt16();
			unk36 = reader.ReadInt16();
			unk38 = reader.ReadInt32();
			swizzled = reader.ReadInt32();

			Size = new Size(width, height);
			Data = reader.ReadBytes(dataLength);
			Palette = reader.ReadBytes(palLength);
		}

		public void Save(Stream stream)
		{
			if (!stream.CanWrite || !stream.CanSeek)
				throw new InvalidDataException($"Write or seek must be supported.");

			var writer = new BinaryWriter(stream);
			var baseOffset = (int)stream.Position;

			var dataOffset = baseOffset + 0x40;
			var palOffset = dataOffset + Data.Length;

			writer.Write(MagicCode);
			writer.Write(unk04);
			writer.Write(dataOffset);
			writer.Write(Data.Length);
			writer.Write(palOffset);
			writer.Write(Palette.Length);
			writer.Write(unk18);
			writer.Write((short)Size.Width);
			writer.Write((short)Size.Height);
			writer.Write(unk20);
			writer.Write(unk22);
			writer.Write(unk24);
			writer.Write(format);
			writer.Write(unk28);
			writer.Write(unk2c);
			writer.Write(unk2e);
			writer.Write(unk30);
			writer.Write(unk34);
			writer.Write(unk36);
			writer.Write(unk38);
			writer.Write(swizzled);

			writer.Write(Data, 0, Data.Length);
			writer.Write(Palette, 0, Palette.Length);
		}

		public Size Size { get; }

		public byte[] Data { get; }

		public byte[] Palette { get; }

		public byte[] GetBitmap()
		{
			switch (format)
			{
				case 0x13:
					return GetBitmapFrom8bpp(
						swizzled == 7 ? Decode8(Encode32(Data, Size.Width / 128, Size.Height / 64), Size.Width / 128, Size.Height / 64) : Data
						, Palette, Size.Width, Size.Height);
				case 0x14:
					return GetBitmapFrom4bpp(
						swizzled == 7 ? Decode4(Encode32(Data, Size.Width / 128, Size.Height / 128), Size.Width / 128, Size.Height / 128) : Data
						, Palette, Size.Width, Size.Height);
				default:
					throw new NotSupportedException($"The format {format} is not supported.");
			}
		}

		private static byte[] GetBitmapFrom8bpp(byte[] src, byte[] palette, int width, int height)
		{
			var dst = new byte[width * height * 4];
			for (int i = 0; i < dst.Length; i += 4)
			{
				var index = Repl(src[i / 4]);
				dst[i + 0] = (byte)Math.Max(0, palette[index * 4 + 2] * 2 - 1);
				dst[i + 1] = (byte)Math.Max(0, palette[index * 4 + 1] * 2 - 1);
				dst[i + 2] = (byte)Math.Max(0, palette[index * 4 + 0] * 2 - 1);
				dst[i + 3] = (byte)Math.Max(0, palette[index * 4 + 3] * 2 - 1);
			}

			return dst;
		}

		private static byte[] GetBitmapFrom4bpp(byte[] src, byte[] palette, int width, int height)
		{
			var dst = new byte[width * height * 4];
			for (int i = 0; i < dst.Length; i += 8)
			{
				var index = src[i / 8] & 0x0F;
				dst[i + 0] = palette[index * 4 + 0];
				dst[i + 1] = palette[index * 4 + 1];
				dst[i + 2] = palette[index * 4 + 2];
				dst[i + 3] = palette[index * 4 + 3];

				index = src[i / 4] >> 4;
				dst[i + 4] = palette[index * 4 + 0];
				dst[i + 5] = palette[index * 4 + 1];
				dst[i + 6] = palette[index * 4 + 2];
				dst[i + 7] = palette[index * 4 + 3];
			}

			return dst;
		}
	}
}
