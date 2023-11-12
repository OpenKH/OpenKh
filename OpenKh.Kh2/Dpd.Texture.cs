using OpenKh.Common;
using System;
using System.Drawing;
using System.IO;

namespace OpenKh.Kh2
{
    public partial class Dpd
    {
		public class Texture
		{
			private short shTexDbp;
            private short shCltDbp;
            private short shDbw;
            private short format; //shDpsm
            private short shX;
            private short shY;
            private short width;
            private short height;
            private uint unTex0L;
			private uint unTex0H;
            private int unClutStart;
            private short shTexVramSize;
            private short shCltVramSize;
            public byte[] Data { get; }
            public byte[] Palette { get; }

            public Texture()
			{

			}

            internal Texture(Stream textureStream)
            {
                shTexDbp = textureStream.ReadInt16();
                shCltDbp = textureStream.ReadInt16();
                shDbw = textureStream.ReadInt16();
                format = textureStream.ReadInt16();
                shX = textureStream.ReadInt16();
                shY = textureStream.ReadInt16();
                width = textureStream.ReadInt16();
                height = textureStream.ReadInt16();
                unTex0L = textureStream.ReadUInt32();
                unTex0H = textureStream.ReadUInt32();
                unClutStart = textureStream.ReadInt32();
                shTexVramSize = textureStream.ReadInt16();
                shCltVramSize = textureStream.ReadInt16();

                Data = textureStream.ReadBytes(width * height);
                Palette = textureStream.ReadBytes(0x100 * sizeof(int));
            }

            internal Texture(BinaryReader reader)
			{
				var basePosition = reader.BaseStream.Position;
				shTexDbp = reader.ReadInt16();
				shCltDbp = reader.ReadInt16();
				shDbw = reader.ReadInt16();
				format = reader.ReadInt16();
				shX = reader.ReadInt16();
				shY = reader.ReadInt16();
				width = reader.ReadInt16();
				height = reader.ReadInt16();
				unTex0L = reader.ReadUInt32();
				unTex0H = reader.ReadUInt32();
				unClutStart = reader.ReadInt32();
				shTexVramSize = reader.ReadInt16();
                shCltVramSize = reader.ReadInt16();

                Data = reader.ReadBytes(width * height);
				Palette = reader.ReadBytes(0x100 * sizeof(int));
			}

			internal void Write(BinaryWriter writer)
			{
				writer.Write(shTexDbp);
				writer.Write(shCltDbp);
				writer.Write(shDbw);
				writer.Write(format);
				writer.Write(shX);
				writer.Write(shY);
				writer.Write(width);
				writer.Write(height);
				writer.Write(unTex0L);
				writer.Write(unTex0H);
				writer.Write(unClutStart);
				writer.Write(shTexVramSize);
				writer.Write(shCltVramSize);

				writer.Write(Data);
				writer.Write(Palette);
			}

            public Stream getAsStream()
            {
                MemoryStream fileStream = new MemoryStream();

                BinaryWriter writer = new BinaryWriter(fileStream, System.Text.Encoding.UTF8, true);

                writer.Write(shTexDbp);
                writer.Write(shCltDbp);
                writer.Write(shDbw);
                writer.Write(format);
                writer.Write(shX);
                writer.Write(shY);
                writer.Write(width);
                writer.Write(height);
                writer.Write(unTex0L);
                writer.Write(unTex0H);
                writer.Write(unClutStart);
                writer.Write(shTexVramSize);
                writer.Write(shCltVramSize);

                writer.Write(Data);
                writer.Write(Palette);

                fileStream.Position = 0;

                return fileStream;
            }

            public Size Size => new Size(width, height);

			public byte[] GetBitmap()
			{
				var swizzled = 0;

				switch (format)
				{
					case 0x13:
						return GetBitmapFrom8bpp(
							swizzled == 7 ? Ps2.Decode8(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 64), Size.Width / 128, Size.Height / 64) : Data
							, Palette, Size.Width, Size.Height);
					case 0x14:
						return GetBitmapFrom4bpp(
							swizzled == 7 ? Ps2.Decode4(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 128), Size.Width / 128, Size.Height / 128) : Data
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
					var index = Ps2.Repl(src[i / 4]);
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

					index = src[i / 8] >> 4;
					dst[i + 4] = palette[index * 4 + 0];
					dst[i + 5] = palette[index * 4 + 1];
					dst[i + 6] = palette[index * 4 + 2];
					dst[i + 7] = palette[index * 4 + 3];
				}

				return dst;
			}
		}
	}
}
