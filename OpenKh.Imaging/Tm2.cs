using OpenKh.Common;
using System;
using System.Drawing;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Imaging
{
    public class Tm2 : IImageRead
    {
		private const uint MagicCode = 0x324D4954U;
        private const int MinimumLength = 16;

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
		private class GsTex0
		{
            [Data] public long data { get; set; }

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
		private class Picture
		{
			[Data] public int size { get; set; }
			[Data] public int palSize { get; set; }
			[Data] public int imgSize { get; set; }
			[Data] public short headSize { get; set; }
			[Data] public short howPal { get; set; }
			[Data] public short howPalUsed { get; set; }
			[Data] public byte palFormat { get; set; }
			[Data] public byte imgFormat { get; set; }
			[Data] public short width { get; set; }
			[Data] public short height { get; set; }
			[Data] public GsTex0 gstex1 { get; set; }
			[Data] public GsTex0 gstex2 { get; set; }
			[Data] public int gsreg { get; set; }
			[Data] public int gspal { get; set; }

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
		};

        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public short Version { get; set; }
            [Data] public short ImageCount { get; set; }
            [Data] public short Unknown08 { get; set; }
            [Data] public short Unknown0a { get; set; }
            [Data] public short Unknown0c { get; set; }
            [Data] public short Unknown0e { get; set; }
        }

		private readonly Picture pic = new Picture();
		private readonly byte[] imgData;
		private readonly byte[] palData;

        public Size Size => new Size(pic.width, pic.height);

        public PixelFormat PixelFormat => pic.ImageFormat;

        public Tm2(Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new InvalidDataException($"Read or seek must be supported.");

            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));

			var reader = new BinaryReader(stream);
			if (stream.Length < MinimumLength || header.MagicCode != MagicCode)
				throw new InvalidDataException("Invalid header");

            pic = BinaryMapping.ReadObject<Picture>(stream);

			var imgPos = (int)stream.Position;
			var palPos = imgPos = pic.imgSize;
			imgData = reader.ReadBytes(pic.imgSize);
			palData = reader.ReadBytes(pic.palSize);

            InvertRedBlueChannels(imgData, pic.ImageFormat);

        }

        public static bool IsValid(Stream stream) =>
            stream.SetPosition(0).ReadInt32() == MagicCode &&
            stream.Length >= MinimumLength;

        public static Tm2 Read(Stream stream) => new Tm2(stream.SetPosition(0));

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

        public byte[] GetData() => imgData;
        public byte[] GetClut() => palData;
    }
}
