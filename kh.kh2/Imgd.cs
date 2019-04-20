using kh.Imaging;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace kh.kh2
{
    public partial class Imgd : IImageRead
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
			Clut = reader.ReadBytes(palLength);
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
			writer.Write(Clut.Length);
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
			writer.Write(Clut, 0, Clut.Length);
		}

		public Size Size { get; }

		public byte[] Data { get; }

		public byte[] Clut { get; }

        public PixelFormat PixelFormat
        {
            get
            {
                switch (format)
                {
                    case 0x13: return PixelFormat.Indexed8;
                    case 0x14: return PixelFormat.Indexed4;
                    default: return PixelFormat.Undefined;
                }
            }
        }

        public byte[] GetData()
		{
			switch (format)
			{
				case 0x13:
                    return swizzled == 7 ? Ps2.Decode8(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 64), Size.Width / 128, Size.Height / 64) : Data;
				case 0x14:
					return swizzled == 7 ? Ps2.Decode4(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 128), Size.Width / 128, Size.Height / 128) : Data;
				default:
					throw new NotSupportedException($"The format {format} is not supported.");
			}
		}

        public byte[] GetClut() => Clut
            .Select(x => (byte)Math.Min(byte.MaxValue, x * 2))
            .ToArray();
	}
}
