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
        private const int Format = 0;
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

        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public byte Version { get; set; }
            [Data] public byte Format { get; set; }
            [Data] public short ImageCount { get; set; }
            [Data] public long Zero { get; set; }
        }

        private class Picture
		{
			[Data] public int TotalSize { get; set; }
			[Data] public int ClutSize { get; set; }
			[Data] public int ImageSize { get; set; }
			[Data] public short HeaderSize { get; set; }
			[Data] public short ClutColors { get; set; }
            [Data] public byte PictureFormat { get; set; }
            [Data] public byte MipMapCount { get; set; }
			[Data] public byte ClutType { get; set; }
			[Data] public byte ImageType { get; set; }
			[Data] public short Width { get; set; }
			[Data] public short Height { get; set; }
			[Data] public GsTex GsTex0 { get; set; }
			[Data] public GsTex GsTex1 { get; set; }
			[Data] public int GsRegs { get; set; }
			[Data] public int GsClut { get; set; }
		};

        private class MipMap
        {
            [Data] public int GsMiptbp1_1 { get; set; }
            [Data] public int GsMiptbp1_2 { get; set; }
            [Data] public int GsMiptbp2_1 { get; set; }
            [Data] public int GsMiptbp2_2 { get; set; }
            [Data(Count = 8)] public int[] Sizes { get; set; }
        }

        private readonly byte _imageFormat;
        private readonly byte _mipMapCount;
        private readonly byte _imageType;
        private readonly byte _clutType;
        private readonly GsTex _gsTex0;
        private readonly GsTex _gsTex1;
        private readonly int _gsReg;
        private readonly int _gsPal;
        private readonly byte[] _imageData;
		private readonly byte[] _clutData;

        public Size Size { get; }

        public PixelFormat PixelFormat => GetPixelFormat(_imageType);

        private Tm2(Stream stream, Picture picture)
		{
            _imageFormat = picture.PictureFormat;
            _mipMapCount = picture.MipMapCount;
            _imageType = picture.ImageType;
            _clutType = picture.ClutType;
            _gsTex0 = picture.GsTex0;
            _gsTex1 = picture.GsTex1;
            _gsReg = picture.GsRegs;
            _gsPal = picture.GsClut;
            Size = new Size(picture.Width, picture.Height);

			_imageData = stream.ReadBytes(picture.ImageSize);
			_clutData = stream.ReadBytes(picture.ClutSize);

            ImageDataHelpers.InvertRedBlueChannels(_imageData, Size, PixelFormat);
        }

        public static bool IsValid(Stream stream) =>
            stream.SetPosition(0).ReadInt32() == MagicCode &&
            stream.Length >= HeaderLength;

        public static IEnumerable<Tm2> Read(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));
            if (header.Format != 0)
                stream.Position = 128;

            if (stream.Length < HeaderLength || header.MagicCode != MagicCode)
                throw new InvalidDataException("Invalid header");

            return Enumerable.Range(0, header.ImageCount)
                .Select(x => new Tm2(stream, BinaryMapping.ReadObject<Picture>(stream)))
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
                Format = Format,
                ImageCount = (short)myImages.Length,
                Zero = 0,
            });

            foreach (var image in myImages)
            {
                var colorCount = image._clutData.Length > 0 ? image._clutData.Length * 8 / GetBitsPerPixel(image._clutType) : 0;

                BinaryMapping.WriteObject(stream, new Picture
                {
                    TotalSize = 0x30 + image._imageData.Length + image._clutData.Length,
                    ClutSize = image._clutData.Length,
                    ImageSize = image._imageData.Length,
                    HeaderSize = 0x30,
                    ClutColors = (short)colorCount,
                    PictureFormat = image._imageFormat,
                    MipMapCount = image._mipMapCount,
                    ClutType = image._clutType,
                    ImageType = image._imageType,
                    Width = (short)image.Size.Width,
                    Height = (short)image.Size.Height,
                    GsTex0 = new GsTex(image._gsTex0),
                    GsTex1 = new GsTex(image._gsTex1),
                    GsRegs = image._gsReg,
                    GsClut = image._gsPal,
                });

                var data = ImageDataHelpers.GetInvertedRedBlueChannels(image._imageData, image.Size, image.PixelFormat);
                stream.Write(data, 0, image._imageData.Length);
                stream.Write(image._clutData, 0, image._clutData.Length);
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
