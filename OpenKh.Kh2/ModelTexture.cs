using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class ModelTexture
    {
        private class Header
        {
            [Data] public int Unk00 { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int TextureCountWcx { get; set; }
            [Data] public int TextureCountWcy { get; set; }
            [Data] public int Offset1 { get; set; }
            [Data] public int Texinf1off { get; set; }
            [Data] public int Texinf2off { get; set; }
            [Data] public int PictureOffset { get; set; }
            [Data] public int PaletteOffset { get; set; }
        }

        public class Texture : IImageRead
        {
            private readonly byte[] _data;
            private readonly byte[] _palette;

            public Texture(int width, int height, PixelFormat pixelFormat, byte[] data, byte[] palette)
            {
                Size = new Size(width, height);
                PixelFormat = pixelFormat;
                _data = data;
                _palette = palette;
            }

            public Size Size { get; }

            public PixelFormat PixelFormat { get; }

            public byte[] GetClut()
            {
                switch (PixelFormat)
                {
                    case PixelFormat.Indexed8: return GetClut8(_palette);
                    case PixelFormat.Indexed4: return GetClut4(_palette);
                    default:
                        throw new NotSupportedException($"The format {PixelFormat} is not supported or does not contain any palette.");
                }
            }

            public byte[] GetData()
            {
                switch (PixelFormat)
                {
                    case PixelFormat.Rgba8888:
                    case PixelFormat.Rgbx8888:
                        return GetData32bpp(_data);
                    case PixelFormat.Indexed8:
                        return Ps2.Decode8(Ps2.Encode32(_data, Size.Width / 128, Size.Height / 64), Size.Width / 128, Size.Height / 64);
                    case PixelFormat.Indexed4:
                        return Ps2.Decode4(Ps2.Encode32(_data, Size.Width / 128, Size.Height / 128), Size.Width / 128, Size.Height / 128);
                    default:
                        throw new NotSupportedException($"The format {PixelFormat} is not supported.");
                }
            }
        }

        private class TexInf1
        {
            public int Data24 { get; set; }
            public int Data28 { get; set; }
            public int Data40 { get; set; }
            public int Data44 { get; set; }
            public int Data60 { get; set; }
            public int Data70 { get; set; }
            public int PictureOffset { get; set; }
            public int Data7c { get; set; }
            public int Data80 { get; set; }
        }

        private class _TexInf1
        {
            [Data] public int Data00 { get; set; }
            [Data] public int Data04 { get; set; }
            [Data] public int Data08 { get; set; }
            [Data] public int Data0c { get; set; }
            [Data] public int Data10 { get; set; }
            [Data] public int Data14 { get; set; }
            [Data] public int Data18 { get; set; }
            [Data] public int Data1c { get; set; }
            [Data] public int Data20 { get; set; }
            [Data] public int Data24 { get; set; }
            [Data] public int Data28 { get; set; }
            [Data] public int Data2c { get; set; }
            [Data] public int Data30 { get; set; }
            [Data] public int Data34 { get; set; }
            [Data] public int Data38 { get; set; }
            [Data] public int Data3c { get; set; }
            [Data] public int Data40 { get; set; }
            [Data] public int Data44 { get; set; }
            [Data] public int Data48 { get; set; }
            [Data] public int Data4c { get; set; }
            [Data] public int Data50 { get; set; }
            [Data] public int Data54 { get; set; }
            [Data] public int Data58 { get; set; }
            [Data] public int Data5c { get; set; }
            [Data] public int Data60 { get; set; }
            [Data] public int Data64 { get; set; }
            [Data] public int Data68 { get; set; }
            [Data] public int Data6c { get; set; }
            [Data] public int Data70 { get; set; }
            [Data] public int PictureOffset { get; set; }
            [Data] public int Data78 { get; set; }
            [Data] public int Data7c { get; set; }
            [Data] public int Data80 { get; set; }
            [Data] public int Data84 { get; set; }
            [Data] public int Data88 { get; set; }
            [Data] public int Data8c { get; set; }
        }

        private class TexInf2
        {
            public long Data30 { get; set; }
            public long Data40 { get; set; }
            public long Data50 { get; set; }
            public long Data60 { get; set; }
            public Tm2.GsTex GsTex0 { get; set; }
            public long Data78 { get; set; }
            public long Data80 { get; set; }
        }

        private class _TexInf2
        {
            [Data] public long Data00 { get; set; }
            [Data] public long Data08 { get; set; }
            [Data] public long Data10 { get; set; }
            [Data] public long Data18 { get; set; }
            [Data] public long Data20 { get; set; }
            [Data] public long Data28 { get; set; }
            [Data] public long Data30 { get; set; }
            [Data] public long Data38 { get; set; }
            [Data] public long Data40 { get; set; }
            [Data] public long Data48 { get; set; }
            [Data] public long Data50 { get; set; }
            [Data] public long Data58 { get; set; }
            [Data] public long Data60 { get; set; }
            [Data] public long Data68 { get; set; }
            [Data] public Tm2.GsTex GsTex0 { get; set; }
            [Data] public long Data78 { get; set; }
            [Data] public long Data80 { get; set; }
            [Data] public long Data88 { get; set; }
            [Data] public long Data90 { get; set; }
            [Data] public long Data98 { get; set; }
        }

        public List<Texture> Images { get; }

        [Data] public int TextureCountWcx { get; }
        [Data] public int TextureCountWcy { get; }

        private List<TexInf1> TexInfo1 { get; }
        private List<TexInf2> TexInfo2 { get; }

        private byte[] OffsetData { get; }
        private byte[] PictureData { get; }
        private byte[] PaletteData { get; }
        private byte[] FooterData { get; }

        private ModelTexture(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));
            if (header.Unk00 == -1)
                return;

            TextureCountWcx = header.TextureCountWcx;
            TextureCountWcy = header.TextureCountWcy;

            var offset1Size = header.Texinf1off - header.Offset1;
            var Texinf2offSize = header.PictureOffset - header.Texinf2off;
            var pictureSize = header.PaletteOffset - header.PictureOffset;
            var paletteSize = header.Unk04 * 4;
            var footerSize = (int)stream.Length - (header.PaletteOffset + paletteSize);
            if (footerSize < 0)
                throw new NotFiniteNumberException("Invalid texture");

            stream.Position = header.Offset1;
            OffsetData = stream.ReadBytes(header.TextureCountWcy);

            stream.Position = header.Texinf1off;
            TexInfo1 = Enumerable.Range(0, header.TextureCountWcx + 1)
                .Select(_ => BinaryMapping.ReadObject<_TexInf1>(stream))
                .Select(x => new TexInf1
                {
                    Data24 = x.Data24,
                    Data28 = x.Data28,
                    Data40 = x.Data40,
                    Data44 = x.Data44,
                    Data60 = x.Data60,
                    Data70 = x.Data70,
                    PictureOffset = x.PictureOffset,
                    Data7c = x.Data7c,
                    Data80 = x.Data80
                })
                .ToList();

            stream.Position = header.Texinf2off;
            TexInfo2 = Enumerable.Range(0, header.TextureCountWcy)
                .Select(_ => BinaryMapping.ReadObject<_TexInf2>(stream))
                .Select(x => new TexInf2
                {
                    Data30 = x.Data30,
                    Data40 = x.Data40,
                    Data50 = x.Data50,
                    Data60 = x.Data60,
                    GsTex0 = x.GsTex0,
                    Data78 = x.Data78,
                    Data80 = x.Data80
                })
                .ToList();

            stream.Position = header.PictureOffset;
            PictureData = stream.ReadBytes(pictureSize);

            stream.Position = header.PaletteOffset;
            PaletteData = stream.ReadBytes(paletteSize);

            FooterData = stream.ReadBytes(footerSize);

            Images = new List<Texture>();
            for (var i = 0; i < header.TextureCountWcy; i++)
            {
                var texInfo1 = TexInfo1[OffsetData[i]];
                var texInfo2 = TexInfo2[i];
                var gsTex = texInfo2.GsTex0;

                var width = 1 << gsTex.TW;
                var height = 1 << gsTex.TH;
                var pixelFormat = GetPixelFormat(gsTex.PSM);
                var dataLength = width * height / (pixelFormat == PixelFormat.Indexed4 ? 2 : 1);
                var data = stream.SetPosition(texInfo1.PictureOffset).ReadBytes(dataLength);

                var texture = new Texture(width, height, pixelFormat, PictureData, PaletteData);
                Images.Add(texture);

                //Debug.Assert(PictureData.Length == width * height / 2);
            }
        }

        public void Write(Stream stream)
        {
            const int HeaderLength = 0x24;

            stream.Position = HeaderLength;
            stream.Write(OffsetData);
            stream.AlignPosition(0x10);

            var texInfo1Offset = (int)stream.Position;
            foreach (var textureInfo in TexInfo1)
                BinaryMapping.WriteObject(stream, new _TexInf1
                {
                    Data00 = 0x10000006,
                    Data04 = 0x00000000,
                    Data08 = 0x13000000,
                    Data0c = 0x50000006,
                    Data10 = 0x00000004,
                    Data14 = 0x10000000,
                    Data18 = 0x0000000e,
                    Data1c = 0x00000000,
                    Data20 = 0x00000000,
                    Data24 = textureInfo.Data24,
                    Data28 = textureInfo.Data28,
                    Data2c = 0x00000000,
                    Data30 = 0x00000000,
                    Data34 = 0x00000000,
                    Data38 = 0x00000051,
                    Data3c = 0x00000000,
                    Data40 = textureInfo.Data40,
                    Data44 = textureInfo.Data44,
                    Data48 = 0x00000052,
                    Data4c = 0x00000000,
                    Data50 = 0x00000000,
                    Data54 = 0x00000000,
                    Data58 = 0x00000053,
                    Data5c = 0x00000000,
                    Data60 = textureInfo.Data60,
                    Data64 = 0x08000000,
                    Data68 = 0x00000000,
                    Data6c = 0x00000000,
                    Data70 = textureInfo.Data70,
                    PictureOffset = textureInfo.PictureOffset,
                    Data78 = 0x00000000,
                    Data7c = textureInfo.Data7c,
                    Data80 = textureInfo.Data80,
                    Data84 = 0x00000000,
                    Data88 = 0x13000000,
                    Data8c = 0x00000000,
                });

            var texInfo2Offset = (int)stream.Position;
            foreach (var textureInfo in TexInfo2)
                BinaryMapping.WriteObject(stream, new _TexInf2
                {
                    Data00 = 0x0000000010000008,
                    Data08 = 0x5000000813000000,
                    Data10 = 0x1000000000008007,
                    Data18 = 0x000000000000000e,
                    Data20 = 0x0000000000000000,
                    Data28 = 0x000000000000003f,
                    Data30 = textureInfo.Data30,
                    Data38 = 0x0000000000000034,
                    Data40 = textureInfo.Data40,
                    Data48 = 0x0000000000000036,
                    Data50 = textureInfo.Data50,
                    Data58 = 0x0000000000000016,
                    Data60 = textureInfo.Data60,
                    Data68 = 0x0000000000000014,
                    GsTex0 = textureInfo.GsTex0,
                    Data78 = textureInfo.Data78,
                    Data80 = textureInfo.Data80,
                    Data88 = 0x0000000000000008,
                    Data90 = 0x0000000060000000,
                    Data98 = 0x0000000013000000,
                });

            stream.AlignPosition(0x80);

            var pictureOffset = (int)stream.Position;
            stream.Write(PictureData);

            var paletteOffset = (int)stream.Position;
            stream.Write(PaletteData);

            stream.Write(FooterData);

            var writer = new BinaryWriter(stream.SetPosition(0));
            writer.Write(0);
            writer.Write(PaletteData.Length / 4);
            writer.Write(TextureCountWcx);
            writer.Write(TextureCountWcy);
            writer.Write(HeaderLength);
            writer.Write(texInfo1Offset);
            writer.Write(texInfo2Offset);
            writer.Write(pictureOffset);
            writer.Write(paletteOffset);
        }

        private static PixelFormat GetPixelFormat(Tm2.GsPSM psm)
        {
            switch (psm)
            {
                case Tm2.GsPSM.GS_PSMT8: return PixelFormat.Indexed8;
                case Tm2.GsPSM.GS_PSMT4: return PixelFormat.Indexed4;
                default:
                    throw new NotSupportedException($"GsPSM format {psm} not supported");
            }
        }

        private static byte[] GetData32bpp(byte[] data)
        {
            var newData = new byte[data.Length];
            for (var i = 0; i < newData.Length - 3; i += 4)
            {
                newData[i + 0] = data[i + 2];
                newData[i + 1] = data[i + 1];
                newData[i + 2] = data[i + 0];
                newData[i + 3] = Ps2.FromPs2Alpha(data[i + 3]);
            }

            return newData;
        }

        private static byte[] GetClut4(byte[] clut)
        {
            var data = new byte[16 * 4];
            for (var i = 0; i < 16; i++)
            {
                var srcIndex = (i & 7) + (i / 8 * 64);
                data[i * 4 + 0] = clut[srcIndex * 4 + 0];
                data[i * 4 + 1] = clut[srcIndex * 4 + 1];
                data[i * 4 + 2] = clut[srcIndex * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(clut[srcIndex * 4 + 3]);
            }

            return data;
        }

        private static byte[] GetClut8(byte[] clut)
        {
            var data = new byte[256 * 4];
            for (var i = 0; i < 256; i++)
            {
                var srcIndex = Ps2.Repl(i);
                data[i * 4 + 0] = clut[srcIndex * 4 + 0];
                data[i * 4 + 1] = clut[srcIndex * 4 + 1];
                data[i * 4 + 2] = clut[srcIndex * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(clut[srcIndex * 4 + 3]);
            }

            return data;
        }

        public static ModelTexture Read(Stream stream) =>
            new ModelTexture(stream);
    }
}
