using kh.Imaging;
using System;
using System.IO;

namespace kh.kh2
{
    public class Tm2
    {
		private const uint MagicCode = 0x324D4954U;

		private enum GsPSM
		{
			GS_PSMCT32 = 0, // 32bit RGBA
			GS_PSMCT24 = 1,
			GS_PSMCT16 = 2,
			GS_PSMCT16S = 10,
			GS_PSMT8 = 19,
			GS_PSMT4 = 20,
			GS_PSMT8H = 27,
			GS_PSMT4HL = 36,
			GS_PSMT4HH = 44,
			GS_PSMZ32 = 48,
			GS_PSMZ24 = 49,
			GS_PSMZ16 = 50,
			GS_PSMZ16S = 58,
		};

		public enum GsCPSM
		{
			GS_PSMCT32 = 0, // 32bit RGBA
			GS_PSMCT24 = 1,
			GS_PSMCT16 = 2,
			GS_PSMCT16S = 10,
		}

		private enum IMG_TYPE
		{
			IT_RGBA = 3,
			IT_CLUT4 = 4,
			IT_CLUT8 = 5,
		};

		private enum CLT_TYPE
		{
			CT_A1BGR5 = 1,
			CT_XBGR8 = 2,
			CT_ABGR8 = 3,
		};

		/// <summary>
		/// register for image
		/// 14 bit, texture buffer base pointer (address / 256)
		/// 6 bit, texture buffer width (texels / 64)
		/// 6 bit, pixel storage format (0 = 32bit RGBA)
		/// 4 bit, width 2^n
		/// 4 bit, height 2^n
		/// 1 bit, 0 = RGB, 1 = RGBA
		/// 2 bit, texture function (0=modulate, 1=decal, 2=hilight, 3=hilight2)
		/// 14 bit, CLUT buffer base pointer (address / 256)
		/// 4 bit, CLUT storage format
		/// 1 bit, storage mode
		/// 5 bit, offset
		/// 3 bit, load control
		///     
		/// http://forum.xentax.com/viewtopic.php?f=16&t=4501&start=75
		/// </summary>
		private struct GsTex0
		{
			private long data;

			public int TBP0
			{
				get => (int)(data >> 0) & 0x3FFF;
				set => data = (data & ~(0x3FFF << 0)) + (value & 0x3FFF);
			}

			public int TBW
			{
				get => (int)(data >> 14) & 0x3F;
				set => data = (data & ~(0x3F << 14)) + (value & 0x3F);
			}

			public GsPSM PSM
			{
				get => (GsPSM)((data >> 20) & 0x3F);
				set => data = (data & ~(0x3F << 20)) + ((int)value & 0x3F);
			}

			public int TW
			{
				get => (int)(data >> 26) & 0xF;
				set => data = (data & ~(0xF << 26)) + (value & 0xF);
			}

			public int TH
			{
				get => (int)(data >> 30) & 0xF;
				set => data = (data & ~(0xF << 30)) + (value & 0xF);
			}

			public int TCC
			{
				get => (int)(data >> 34) & 1;
				set => data = (data & ~(1 << 34)) + (value & 1);
			}

			public int TFX
			{
				get => (int)(data >> 35) & 3;
				set => data = (data & ~(3 << 35)) + (value & 3);
			}

			public int CBP
			{
				get => (int)(data >> 37) & 0x3FFF;
				set => data = (data & ~(0x3FFF << 37)) + (value & 0x3FFF);
			}

			public GsCPSM CPSM
			{
				get => (GsCPSM)((data >> 51) & 0xF);
				set => data = (data & ~(0xF << 51)) + ((int)value & 0xF);
			}

			public int CSM
			{
				get => (int)(data >> 55) & 1;
				set => data = (data & ~(1 << 55)) + (value & 1);
			}

			public int CSA
			{
				get => (int)(data >> 56) & 0x1F;
				set => data = (data & ~(0x1F << 56)) + (value & 0x1F);
			}

			public int CLD
			{
				get => (int)(data >> 61) & 7;
				set => data = (data & ~(7 << 61)) + (value & 7);
			}

			public void Read(BinaryReader reader)
			{
				data = reader.ReadInt64();
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(data);
			}
		}


		/// <summary>
		/// description of image
		/// 4 byte, total size
		/// 4 byte, palette size
		/// 4 byte, image size
		/// 2 byte, head size
		/// 2 byte, how palettes there are
		/// 2 byte, how palettes are used
		/// 1 byte, palette format
		/// 1 byte, image format
		/// 2 byte, width
		/// 2 byte, height
		/// GsTex0, for two times
		/// 4 byte, gsreg
		/// 4 byte, gspal
		/// </summary>
		private struct TM2Pic
		{
			public int size;
			public int palSize;
			public int imgSize;
			public short headSize;
			public short howPal;
			public short howPalUsed;
			public byte palFormat;
			public byte imgFormat;
			public short width;
			public short height;
			public GsTex0 gstex1;
			public GsTex0 gstex2;
			public int gsreg;
			public int gspal;

			public PixelFormat ImageFormat
			{
				get
				{
					switch (imgFormat)
					{
						case 2: return PixelFormat.Rgb888;
						case 3: return PixelFormat.Rgba8888;
						case 4: return PixelFormat.Indexed4;
						case 5: return PixelFormat.Indexed8;
						default:
							throw new ArgumentOutOfRangeException($"imgFormat {imgFormat} invalid or not supported.");
					}
				}
			}

			public int BitsPerPixel
			{
				get
				{
					switch (imgFormat)
					{
						case 2: return 24;
						case 3: return 32;
						case 4: return 4;
						case 5: return 8;
						default:
							throw new ArgumentOutOfRangeException(nameof(imgFormat), $"{imgFormat} invalid or not supported.");
					}
				}
			}

			public PixelFormat PaletteFormat
			{
				get
				{
					switch (palFormat)
					{
						case 0: return PixelFormat.Undefined;
						case 1: return PixelFormat.Rgba1555;
						case 2: return PixelFormat.Rgbx8888;
						case 3: return PixelFormat.Rgba8888;
						default:
							throw new ArgumentOutOfRangeException(nameof(palFormat), $"{palFormat} invalid or not supported.");
					}
				}
			}

			public void Read(BinaryReader reader)
			{
				size = reader.ReadInt32();
				palSize = reader.ReadInt32();
				imgSize = reader.ReadInt32();
				headSize = reader.ReadInt16();
				howPal = reader.ReadInt16();
				howPalUsed = reader.ReadInt16();
				palFormat = reader.ReadByte();
				imgFormat = reader.ReadByte();
				width = reader.ReadInt16();
				height = reader.ReadInt16();
				gstex1.Read(reader);
				gstex2.Read(reader);
				gsreg = reader.ReadInt32();
				gspal = reader.ReadInt32();
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(size);
				writer.Write(palSize);
				writer.Write(imgSize);
				writer.Write(headSize);
				writer.Write(howPal);
				writer.Write(howPalUsed);
				writer.Write(palFormat);
				writer.Write(imgFormat);
				writer.Write(width);
				writer.Write(height);
				gstex1.Write(writer);
				gstex2.Write(writer);
				writer.Write(gsreg);
				writer.Write(gspal);
			}
		};

		private readonly TM2Pic pic = new TM2Pic();
		private readonly byte[] imgData;
		private readonly byte[] palData;

		public Tm2(Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new InvalidDataException($"Read or seek must be supported.");

			var reader = new BinaryReader(stream);
			if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
				throw new InvalidDataException("Invalid header");

			short version = reader.ReadInt16();
			short imagesCount = reader.ReadInt16();
			int unk08 = reader.ReadInt16();
			int unk0c = reader.ReadInt16();
			pic.Read(reader);

			var imgPos = (int)stream.Position;
			var palPos = imgPos = pic.imgSize;
			imgData = reader.ReadBytes(pic.imgSize);
			palData = reader.ReadBytes(pic.palSize);
		}

		private void InvertRedBlueChannels(byte[] data, PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.Rgb888:
					for (int i = 0; i < pic.width * pic.height; i++)
					{
						byte tmp = data[i * 3 + 0];
						data[i * 3 + 0] = data[i * 3 + 2];
						data[i * 3 + 2] = tmp;
					}
					break;
				case PixelFormat.Rgba8888:
					for (int i = 0; i < pic.width * pic.height; i++)
					{
						byte tmp = data[i * 4 + 0];
						data[i * 4 + 0] = data[i * 4 + 2];
						data[i * 4 + 2] = tmp;
					}
					break;
				case PixelFormat.Indexed4:
					for (int i = 0; i < pic.width * pic.height / 2; i++)
					{
						data[i] = (byte)(((data[i] & 0x0F) << 4) | (data[i] >> 4));
					}
					break;
			}
		}
    }
}
