using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Imaging
{
    public class Tm2 : IImageRead
    {
		private const uint MagicCode = 0x324D4954U;
        private const int Version = 4;
        private const int HeaderLength = 16;

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
		private class GsTex
		{
            public GsTex()
            {

            }

            public GsTex(GsTex gsTex)
            {
                Data = gsTex.Data;
            }

            [Data] public long Data { get; set; }

			public int TBP0
			{
				get => (int)(Data >> 0) & 0x3FFF;
				set => Data = (Data & ~(0x3FFF << 0)) + (value & 0x3FFF);
			}

			public int TBW
			{
				get => (int)(Data >> 14) & 0x3F;
				set => Data = (Data & ~(0x3F << 14)) + (value & 0x3F);
			}

			public GsPSM PSM
			{
				get => (GsPSM)((Data >> 20) & 0x3F);
				set => Data = (Data & ~(0x3F << 20)) + ((int)value & 0x3F);
			}

			public int TW
			{
				get => (int)(Data >> 26) & 0xF;
				set => Data = (Data & ~(0xF << 26)) + (value & 0xF);
			}

			public int TH
			{
				get => (int)(Data >> 30) & 0xF;
				set => Data = (Data & ~(0xF << 30)) + (value & 0xF);
			}

			public int TCC
			{
				get => (int)(Data >> 34) & 1;
				set => Data = (Data & ~(1 << 34)) + (value & 1);
			}

			public int TFX
			{
				get => (int)(Data >> 35) & 3;
				set => Data = (Data & ~(3 << 35)) + (value & 3);
			}

			public int CBP
			{
				get => (int)(Data >> 37) & 0x3FFF;
				set => Data = (Data & ~(0x3FFF << 37)) + (value & 0x3FFF);
			}

			public GsCPSM CPSM
			{
				get => (GsCPSM)((Data >> 51) & 0xF);
				set => Data = (Data & ~(0xF << 51)) + ((int)value & 0xF);
			}

			public int CSM
			{
				get => (int)(Data >> 55) & 1;
				set => Data = (Data & ~(1 << 55)) + (value & 1);
			}

			public int CSA
			{
				get => (int)(Data >> 56) & 0x1F;
				set => Data = (Data & ~(0x1F << 56)) + (value & 0x1F);
			}

			public int CLD
			{
				get => (int)(Data >> 61) & 7;
				set => Data = (Data & ~(7 << 61)) + (value & 7);
			}
		}

		private class Picture
		{
			[Data] public int DataLength { get; set; }
			[Data] public int ClutLength { get; set; }
			[Data] public int ImageLength { get; set; }
			[Data] public short DataOffset { get; set; }
			[Data] public short ColorCount { get; set; }
			[Data] public short ColorUsedCount { get; set; }
			[Data] public byte ClutFormat { get; set; }
			[Data] public byte ImageFormat { get; set; }
			[Data] public short Width { get; set; }
			[Data] public short Height { get; set; }
			[Data] public GsTex GsTex0 { get; set; }
			[Data] public GsTex GsTex1 { get; set; }
			[Data] public int GsReg { get; set; }
			[Data] public int GsPal { get; set; }
		};

        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public short Version { get; set; }
            [Data] public short ImageCount { get; set; }
            [Data] public int Unknown08 { get; set; }
            [Data] public int Unknown0c { get; set; }
        }

        private readonly byte _imageFormat;
        private readonly byte _clutFormat;
        private readonly GsTex _gsTex0;
        private readonly GsTex _gsTex1;
        private readonly int _gsReg;
        private readonly int _gsPal;
        private readonly byte[] _imageData;
		private readonly byte[] _clutData;

        public Size Size { get; }

        public PixelFormat PixelFormat => GetPixelFormat(_imageFormat);

        private Tm2(Stream stream, Picture picture)
		{
            _imageFormat = picture.ImageFormat;
            _clutFormat = picture.ClutFormat;
            _gsTex0 = picture.GsTex0;
            _gsTex1 = picture.GsTex1;
            _gsReg = picture.GsReg;
            _gsPal = picture.GsPal;
            Size = new Size(picture.Width, picture.Height);

            stream.Position = HeaderLength + picture.DataOffset;
			_imageData = stream.ReadBytes(picture.ImageLength);
			_clutData = stream.ReadBytes(picture.ClutLength);

            InvertRedBlueChannels(_imageData, Size, PixelFormat);
        }

        public static bool IsValid(Stream stream) =>
            stream.SetPosition(0).ReadInt32() == MagicCode &&
            stream.Length >= HeaderLength;

        public static IEnumerable<Tm2> Read(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));

            if (stream.Length < HeaderLength || header.MagicCode != MagicCode)
                throw new InvalidDataException("Invalid header");

            return Enumerable.Range(0, header.ImageCount)
                .Select(x => BinaryMapping.ReadObject<Picture>(stream))
                .ToArray()
                .Select(x => new Tm2(stream, x))
                .ToArray();
        }

        public static void Write(Stream stream, IEnumerable<Tm2> images)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new InvalidDataException($"Write or seek must be supported.");

            var myImages = images.ToArray();
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = Version,
                ImageCount = (short)myImages.Length,
                Unknown08 = 0,
                Unknown0c = 0
            });

            foreach (var image in myImages)
            {
                var colorCount = image._clutData.Length > 0 ? image._clutData.Length * 8 / GetBitsPerPixel(image._clutFormat) : 0;

                BinaryMapping.WriteObject(stream, new Picture
                {
                    DataLength = 0x30 + image._imageData.Length + image._clutData.Length,
                    ClutLength = image._clutData.Length,
                    ImageLength = image._imageData.Length,
                    DataOffset = 0x30,
                    ColorCount = (short)colorCount,
                    ColorUsedCount = (short)(colorCount > 0 ? colorCount : 256),
                    ClutFormat = image._clutFormat,
                    ImageFormat = image._imageFormat,
                    Width = (short)image.Size.Width,
                    Height = (short)image.Size.Height,
                    GsTex0 = new GsTex(image._gsTex0),
                    GsTex1 = new GsTex(image._gsTex1),
                    GsReg = image._gsReg,
                    GsPal = image._gsPal,
                });
            }

            foreach (var image in myImages)
            {
                InvertRedBlueChannels(image._imageData, image.Size, image.PixelFormat);
                stream.Write(image._imageData, 0, image._imageData.Length);
                InvertRedBlueChannels(image._imageData, image.Size, image.PixelFormat);

                stream.Write(image._clutData, 0, image._clutData.Length);
            }
        }

        private static void InvertRedBlueChannels(byte[] data, Size size, PixelFormat pixelFormat)
		{
            var length = size.Width * size.Height;
            switch (pixelFormat)
			{
				case PixelFormat.Rgb888:
					for (int i = 0; i < length; i++)
					{
						byte tmp = data[i * 3 + 0];
						data[i * 3 + 0] = data[i * 3 + 2];
						data[i * 3 + 2] = tmp;
					}
					break;
				case PixelFormat.Rgba8888:
					for (int i = 0; i < length; i++)
					{
						byte tmp = data[i * 4 + 0];
						data[i * 4 + 0] = data[i * 4 + 2];
						data[i * 4 + 2] = tmp;
					}
					break;
				case PixelFormat.Indexed4:
					for (int i = 0; i < length / 2; i++)
					{
						data[i] = (byte)(((data[i] & 0x0F) << 4) | (data[i] >> 4));
					}
					break;
			}
		}

        public byte[] GetData() => _imageData;
        public byte[] GetClut() => _clutData;

        private static int GetBitsPerPixel(int format)
        {
            switch (format)
            {
                case 0: return 0;
                case 1: return 16;
                case 2: return 24;
                case 3: return 32;
                case 4: return 4;
                case 5: return 8;
                default:
                    throw new ArgumentOutOfRangeException($"The format ID {format} is invalid or not supported.");
            }
        }

        private static PixelFormat GetPixelFormat(int format)
        {
            switch (format)
            {
                case 0: return PixelFormat.Undefined;
                case 1: return PixelFormat.Rgba1555;
                case 2: return PixelFormat.Rgb888;
                case 3: return PixelFormat.Rgba8888;
                case 4: return PixelFormat.Indexed4;
                case 5: return PixelFormat.Indexed8;
                default:
                    throw new ArgumentOutOfRangeException($"The format ID {format} is invalid or not supported.");
            }
        }
    }
}
