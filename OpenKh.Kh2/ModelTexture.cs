using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class ModelTexture
    {
        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int ColorCount { get; set; }
            [Data] public int TextureInfoCount { get; set; }
            [Data] public int GsInfoCount { get; set; }
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
            private readonly int _cbp;
            private readonly int _csa;
            private readonly Tm2.GsPSM _uploadPixelFormat;

            internal Texture(
                int width, int height, PixelFormat pixelFormat, Tm2.GsPSM uploadPixelFormat,
                byte[] data, byte[] palette,
                TextureAddressMode textureAddressMode, int cbp, int csa)
            {
                Size = new Size(width, height);
                PixelFormat = pixelFormat;
                _uploadPixelFormat = uploadPixelFormat;
                _data = data;
                _palette = palette;
                TextureAddressMode = textureAddressMode;
                _cbp = cbp;
                _csa = csa;
            }

            public Size Size { get; }

            public PixelFormat PixelFormat { get; }

            public TextureAddressMode TextureAddressMode { get; }

            public byte[] GetClut()
            {
                switch (PixelFormat)
                {
                    case PixelFormat.Indexed8: return GetClut8(_palette, _cbp, _csa);
                    case PixelFormat.Indexed4: return GetClut4(_palette, _cbp, _csa);
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
                        if (_uploadPixelFormat == Tm2.GsPSM.GS_PSMCT32)
                            return Ps2.Decode8(Ps2.Encode32(_data, Size.Width / 128, Size.Height / 64), Size.Width / 128, Size.Height / 64);
                        return _data;
                    case PixelFormat.Indexed4:
                        if (_uploadPixelFormat == Tm2.GsPSM.GS_PSMCT32)
                            return Ps2.Decode4(Ps2.Encode32(_data, Size.Width / 128, Size.Height / 128), Size.Width / 128, Size.Height / 128);
                        return _data;
                    default:
                        throw new NotSupportedException($"The format {PixelFormat} is not supported.");
                }
            }
        }

        private class DataTransferInfo
        {
            public DataTransferInfo()
            {

            }

            public BITBLTBUF BitBltBuf { get; set; }
            public TRXPOS TrxPos { get; set; }
            public TRXREG TrxReg { get; set; }
            public TRXDIR TrxDir { get; set; }
            public int DataOffset { get; set; }

            public int QuadWordCount { get; set; }
        }

        private class _DataTransfer
        {
            [Data] public int Data00 { get; set; }
            [Data] public int Data04 { get; set; }
            [Data] public int Data08 { get; set; }
            [Data] public int Data0c { get; set; }
            [Data] public int Data10 { get; set; }
            [Data] public int Data14 { get; set; }
            [Data] public int Data18 { get; set; }
            [Data] public int Data1c { get; set; }
            [Data] public BITBLTBUF BitBltBuf { get; set; }
            [Data] public long GSReg50 { get; set; }
            [Data] public TRXPOS TrxPos { get; set; }
            [Data] public long GSReg51 { get; set; }
            [Data] public TRXREG TrxReg { get; set; }
            [Data] public long GSReg52 { get; set; }
            [Data] public TRXDIR TrxDir { get; set; }
            [Data] public long GSReg53 { get; set; }
            [Data] public int Data60 { get; set; }
            [Data] public int Data64 { get; set; }
            [Data] public int Data68 { get; set; }
            [Data] public int Data6c { get; set; }
            [Data] public int Data70 { get; set; }
            [Data] public int DataOffset { get; set; }
            [Data] public int Data78 { get; set; }
            [Data] public int Data7c { get; set; }
            [Data] public int Data80 { get; set; }
            [Data] public int Data84 { get; set; }
            [Data] public int Data88 { get; set; }
            [Data] public int Data8c { get; set; }

            public int QuadWordCount => Data60 & 0x7FFF;

            public DataTransferInfo ToInfo() => new DataTransferInfo
            {
                BitBltBuf = BitBltBuf,
                TrxPos = TrxPos,
                TrxReg = TrxReg,
                TrxDir = TrxDir,
                DataOffset = DataOffset,
                QuadWordCount = QuadWordCount,
            };

        }

        public class GsInfo
        {
            public MIPTBP1 MipTbp1 { get; set; }
            public MIPTBP2 MipTbp2 { get; set; }
            public Tm2.GsTex Tex2 { get; set; }
            public TEX1 Tex1 { get; set; }
            public Tm2.GsTex Tex0 { get; set; }
            public TextureAddressMode AddressMode { get; set; }
        }

        private class _GsInfo
        {
            [Data] public long Data00 { get; set; }
            [Data] public long Data08 { get; set; }
            [Data] public long Data10 { get; set; }
            [Data] public long Data18 { get; set; }
            [Data] public long TexFlush { get; set; }
            [Data] public long GSReg3F { get; set; }
            [Data] public MIPTBP1 MipTbp1 { get; set; }
            [Data] public long GSReg34 { get; set; }
            [Data] public MIPTBP2 MipTbp2 { get; set; }
            [Data] public long GSReg36 { get; set; }
            [Data] public Tm2.GsTex Tex2 { get; set; }
            [Data] public long GSReg16 { get; set; }
            [Data] public TEX1 Tex1 { get; set; }
            [Data] public long GSReg14 { get; set; }
            [Data] public Tm2.GsTex Tex0 { get; set; }
            [Data] public long GSReg06 { get; set; }
            [Data] public CLAMP Clamp { get; set; }
            [Data] public long Data88 { get; set; }
            [Data] public long Data90 { get; set; }
            [Data] public long Data98 { get; set; }
        }

        public enum TextureWrapMode
        {
            Repeat, Clamp, RegionClamp, RegionRepeat
        }

        public class TextureAddressMode
        {
            public TextureWrapMode AddressU { get; set; }
            public TextureWrapMode AddressV { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public class BITBLTBUF
        {
            [Data] public long Data { get; set; }

            public BITBLTBUF() { }
            public BITBLTBUF(long data) { Data = data; }

            public int SBP { get => GetBits(Data, 0, 14); set => Data = SetBits(Data, 0, 14, value); }
            public int SBW { get => GetBits(Data, 16, 6); set => Data = SetBits(Data, 16, 6, value); }
            public int SPSM { get => GetBits(Data, 24, 6); set => Data = SetBits(Data, 24, 6, value); }
            public int DBP { get => GetBits(Data, 32, 14); set => Data = SetBits(Data, 32, 14, value); }
            public int DBW { get => GetBits(Data, 48, 6); set => Data = SetBits(Data, 48, 6, value); }
            public int DPSM { get => GetBits(Data, 56, 6); set => Data = SetBits(Data, 56, 6, value); }
        }

        public class TRXPOS
        {
            [Data] public long Data { get; set; }

            public TRXPOS() { }
            public TRXPOS(long data) { Data = data; }

            public int SSAX { get => GetBits(Data, 0, 11); set => Data = SetBits(Data, 0, 11, value); }
            public int SSAY { get => GetBits(Data, 16, 11); set => Data = SetBits(Data, 16, 11, value); }
            public int DSAX { get => GetBits(Data, 32, 11); set => Data = SetBits(Data, 32, 11, value); }
            public int DSAY { get => GetBits(Data, 48, 11); set => Data = SetBits(Data, 48, 11, value); }
            public int DIR { get => GetBits(Data, 59, 2); set => Data = SetBits(Data, 59, 2, value); }
        }

        public class TRXREG
        {
            [Data] public long Data { get; set; }

            public TRXREG() { }
            public TRXREG(long data) { Data = data; }

            public int RRW { get => GetBits(Data, 0, 12); set => Data = SetBits(Data, 0, 12, value); }
            public int RRH { get => GetBits(Data, 32, 12); set => Data = SetBits(Data, 32, 12, value); }
        }

        public class TRXDIR
        {
            [Data] public long Data { get; set; }

            public TRXDIR() { }
            public TRXDIR(long data) { Data = data; }

            public int XDIR { get => GetBits(Data, 0, 2); set => Data = SetBits(Data, 0, 2, value); }
        }

        public class MIPTBP1
        {
            [Data] public long Data { get; set; }

            public MIPTBP1() { }
            public MIPTBP1(long data) { Data = data; }

            public int TBP1 { get => GetBits(Data, 0, 14); set => Data = SetBits(Data, 0, 14, value); }
            public int TBW1 { get => GetBits(Data, 14, 6); set => Data = SetBits(Data, 14, 6, value); }
            public int TBP2 { get => GetBits(Data, 20, 14); set => Data = SetBits(Data, 20, 14, value); }
            public int TBW2 { get => GetBits(Data, 34, 6); set => Data = SetBits(Data, 34, 6, value); }
            public int TBP3 { get => GetBits(Data, 40, 14); set => Data = SetBits(Data, 40, 14, value); }
            public int TBW3 { get => GetBits(Data, 54, 6); set => Data = SetBits(Data, 54, 6, value); }
        }

        public class MIPTBP2
        {
            [Data] public long Data { get; set; }

            public MIPTBP2() { }
            public MIPTBP2(long data) { Data = data; }

            public int TBP4 { get => GetBits(Data, 0, 14); set => Data = SetBits(Data, 0, 14, value); }
            public int TBW4 { get => GetBits(Data, 14, 6); set => Data = SetBits(Data, 14, 6, value); }
            public int TBP5 { get => GetBits(Data, 20, 14); set => Data = SetBits(Data, 20, 14, value); }
            public int TBW5 { get => GetBits(Data, 34, 6); set => Data = SetBits(Data, 34, 6, value); }
            public int TBP6 { get => GetBits(Data, 40, 14); set => Data = SetBits(Data, 40, 14, value); }
            public int TBW6 { get => GetBits(Data, 54, 6); set => Data = SetBits(Data, 54, 6, value); }
        }

        public class TEX1
        {
            [Data] public long Data { get; set; }

            public TEX1() { }
            public TEX1(long data) { Data = data; }

            public int LCM { get => GetBits(Data, 0, 1); set => Data = SetBits(Data, 0, 1, value); }
            public int MXL { get => GetBits(Data, 2, 3); set => Data = SetBits(Data, 2, 3, value); }
            public int MMAG { get => GetBits(Data, 5, 1); set => Data = SetBits(Data, 5, 1, value); }
            public int MMIN { get => GetBits(Data, 6, 3); set => Data = SetBits(Data, 6, 3, value); }
            public int MTBA { get => GetBits(Data, 9, 1); set => Data = SetBits(Data, 9, 1, value); }
            public int L { get => GetBits(Data, 19, 2); set => Data = SetBits(Data, 19, 2, value); }
            public int K { get => GetBits(Data, 32, 12); set => Data = SetBits(Data, 32, 12, value); }
        }

        public class CLAMP
        {
            [Data] public long Data { get; set; }

            public CLAMP() { }
            public CLAMP(long data) { Data = data; }

            public int WMS { get => GetBits(Data, 0, 2); set => Data = SetBits(Data, 0, 2, value); }
            public int WMT { get => GetBits(Data, 2, 2); set => Data = SetBits(Data, 2, 2, value); }
            public int MINU { get => GetBits(Data, 4, 10); set => Data = SetBits(Data, 4, 10, value); }
            public int MAXU { get => GetBits(Data, 14, 10); set => Data = SetBits(Data, 14, 10, value); }
            public int MINV { get => GetBits(Data, 24, 10); set => Data = SetBits(Data, 24, 10, value); }
            public int MAXV { get => GetBits(Data, 34, 10); set => Data = SetBits(Data, 34, 10, value); }
        }

        class ClutAlloc
        {
            public int CBP;
            public int CSA;

            public int WriteOffset;

            public void Write(Stream paletteData, byte[] clutData)
            {
                paletteData.SetLength(Math.Max(WriteOffset + 4 * 256, paletteData.Length));

                for (int index = 0, count = clutData.Length / 4; index < count; index++)
                {
                    var srcOffset = 4 * (index);
                    var dstOffset = 4 * GetClutPointer(index, WriteOffset / 256, CSA);
                    paletteData.Position = dstOffset;
                    paletteData.WriteByte(clutData[srcOffset]);
                    paletteData.WriteByte(clutData[srcOffset + 1]);
                    paletteData.WriteByte(clutData[srcOffset + 2]);
                    paletteData.WriteByte(ToPs2Alpha(clutData[srcOffset + 3]));
                }
            }

            private static byte ToPs2Alpha(byte data) => (byte)((data + 1) / 2);
        }

        class ClutBuilder
        {
            private int clutBasePtr;
            private int ptr;
            private Queue<ClutAlloc> freeIndexed4 = new Queue<ClutAlloc>();

            public ClutBuilder(int clutBasePtr)
            {
                this.clutBasePtr = clutBasePtr;
            }

            public ClutAlloc Allocate(bool isIndexed4)
            {
                ClutAlloc entry;

                if (isIndexed4)
                {
                    if (!freeIndexed4.Any())
                    {
                        for (int index = 0; index < 16; index++)
                        {
                            freeIndexed4.Enqueue(
                                new ClutAlloc
                                {
                                    CBP = clutBasePtr + ptr,
                                    CSA = index,

                                    WriteOffset = 256 * ptr,
                                }
                            );
                        }
                    }

                    ptr += 4;

                    entry = freeIndexed4.Dequeue();
                }
                else
                {
                    entry = new ClutAlloc
                    {
                        CBP = clutBasePtr + ptr,
                        CSA = 0,

                        WriteOffset = 256 * ptr,
                    };

                    ptr += 4;
                }

                return entry;
            }
        }

        private const int MagicCode = 0;
        private const int HeaderLength = 0x24;

        private DataTransferInfo _clutTransfer;
        private List<DataTransferInfo> _textureTransfer;
        private List<GsInfo> _gsInfo;

        /// <summary>
        /// 0x74 of DataTransfer will be relative to offset from zero position.
        /// </summary>
        private bool _useRelativeOffset;

        public List<Texture> Images { get; }

        private byte[] OffsetData { get; }
        private byte[] PictureData { get; }
        private byte[] PaletteData { get; }
        private byte[] FooterData { get; }

        private const int ClutBasePtr = 0x2C00;
        private const int TexBasePtr = 0x3000;

        private ModelTexture(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));
            if (header.MagicCode == -1)
                return;

            var offset1Size = header.Texinf1off - header.Offset1;
            var Texinf2offSize = header.PictureOffset - header.Texinf2off;
            var pictureSize = header.PaletteOffset - header.PictureOffset;
            var paletteSize = header.ColorCount * 4;
            var footerSize = (int)stream.Length - (header.PaletteOffset + paletteSize);
            if (footerSize < 0)
                throw new NotFiniteNumberException("Invalid texture");

            stream.Position = header.Offset1;
            OffsetData = stream.ReadBytes(header.GsInfoCount);

            stream.Position = header.Texinf1off;
            _clutTransfer = BinaryMapping.ReadObject<_DataTransfer>(stream).ToInfo();
            _textureTransfer = Enumerable.Range(0, header.TextureInfoCount)
                .Select(_ => BinaryMapping.ReadObject<_DataTransfer>(stream).ToInfo())
                .ToList();

            stream.Position = header.Texinf2off;
            _gsInfo = Enumerable.Range(0, header.GsInfoCount)
                .Select(_ => BinaryMapping.ReadObject<_GsInfo>(stream))
                .Select(x => new GsInfo
                {
                    MipTbp1 = x.MipTbp1,
                    MipTbp2 = x.MipTbp2,
                    Tex2 = x.Tex2,
                    Tex1 = x.Tex1,
                    Tex0 = x.Tex0,
                    AddressMode = ClampFromGs(x.Clamp.Data),
                })
                .ToList();

            stream.Position = header.PictureOffset;
            PictureData = stream.ReadBytes(pictureSize);

            stream.Position = header.PaletteOffset;
            PaletteData = stream.ReadBytes(paletteSize);

            FooterData = stream.ReadBytes(footerSize);

            var paletteBaseOffset = _clutTransfer.BitBltBuf.DBP;

            Images = new List<Texture>();
            for (var i = 0; i < header.GsInfoCount; i++)
            {
                var texInfo = _textureTransfer[OffsetData[i]];
                var gsInfo = _gsInfo[i];
                var gsTex = gsInfo.Tex0;

                var width = 1 << gsTex.TW;
                var height = 1 << gsTex.TH;
                var pixelFormat = GetPixelFormat(gsTex.PSM);
                var uploadPixelFormat = (Tm2.GsPSM)texInfo.BitBltBuf.DPSM;
                var dataLength = width * height / (pixelFormat == PixelFormat.Indexed4 ? 2 : 1);
                var data = stream.SetPosition(texInfo.DataOffset).ReadBytes(dataLength);

                Images.Add(new Texture(width, height, pixelFormat, uploadPixelFormat, data, PaletteData, gsInfo.AddressMode, gsTex.CBP - paletteBaseOffset, gsTex.CSA));
            }
        }

        public class Build
        {
            public IList<Imgd> images;
            public byte[] offsetData;
            public IList<UserDataTransferInfo> textureTransfer;
            public IList<UserGsInfo> gsInfo;
        }

        public class UserDataTransferInfo
        {
            public BITBLTBUF BitBltBuf { get; set; } = new BITBLTBUF();
            public TRXPOS TrxPos { get; set; } = new TRXPOS();
            public TRXREG TrxReg { get; set; } = new TRXREG();
            public TRXDIR TrxDir { get; set; } = new TRXDIR();

            public UserDataTransferInfo()
            {

            }

            public UserDataTransferInfo(int width, int height)
            {
                BitBltBuf.DBW = width / 128;
                BitBltBuf.DPSM = (int)Tm2.GsPSM.GS_PSMCT32;
                TrxReg.RRW = width / 2;
                TrxReg.RRH = height / 2;
            }
        }

        public class UserGsInfo
        {
            public MIPTBP1 MipTbp1 { get; set; } = new MIPTBP1();
            public MIPTBP2 MipTbp2 { get; set; } = new MIPTBP2();
            public Tm2.GsTex Tex2 { get; set; } = new Tm2.GsTex();
            public TEX1 Tex1 { get; set; } = new TEX1();
            public Tm2.GsTex Tex0 { get; set; } = new Tm2.GsTex();
            public TextureAddressMode AddressMode { get; set; } = new TextureAddressMode();

            public UserGsInfo()
            {

            }

            public UserGsInfo(Imgd imageInfo)
                : this(imageInfo.Size.Width, imageInfo.Size.Height, ToPSM(imageInfo.PixelFormat))
            {

            }

            public UserGsInfo(int width, int height, Tm2.GsPSM psm)
            {
                Tex2.CPSM = Tm2.GsCPSM.GS_PSMCT32; // RGBA palette
                Tex2.CPSM = Tm2.GsCPSM.GS_PSMCT32; // RGBA palette
                Tex2.CSA = 0; // always 0
                Tex2.CLD = 4;

                Tex1.MMAG = 1;

                Tex0.TBW = width / 64;
                Tex0.PSM = psm;
                Tex0.TW = GetSizeRegister(width);
                Tex0.TH = GetSizeRegister(height);
                Tex0.TCC = true;
                Tex0.CPSM = Tm2.GsCPSM.GS_PSMCT32; // RGBA palette
                Tex0.CSM = false;

                AddressMode.AddressU = TextureWrapMode.RegionClamp;
                AddressMode.AddressV = TextureWrapMode.RegionClamp;
                AddressMode.Left = 0;
                AddressMode.Right = width - 1;
                AddressMode.Top = 0;
                AddressMode.Bottom = height - 1;
            }
        }

        public ModelTexture(IEnumerable<Imgd> images)
            : this(new Build { images = images.ToArray(), })
        {

        }

        public ModelTexture(Build build)
        {
            _useRelativeOffset = true;

            OffsetData = build.offsetData ?? Enumerable.Range(0, build.images.Count)
                .Select(it => Convert.ToByte(it))
                .ToArray();

            var picData = new MemoryStream();
            var palData = new MemoryStream();

            var texTransList = new List<DataTransferInfo>();
            var gsInfoList = new List<GsInfo>();

            var clutBuilder = new ClutBuilder(ClutBasePtr);

            var clutAllocList = new List<ClutAlloc>();

            var userTextureTransfer = build.textureTransfer
                ?? build.images
                    .Select(image => new UserDataTransferInfo(image.Size.Width, image.Size.Height))
                    .ToArray();

            var userGsInfo = build.gsInfo
                ?? OffsetData
                    .Select(index => build.images[index])
                    .Select(image => new UserGsInfo(image))
                    .ToArray();

            foreach (var (image, imageIndex) in build.images.Select((image, imageIn) => (image, imageIn)))
            {
                var encodedImage = new Imgd(image.Size, image.PixelFormat, image.GetData(), image.GetClut(), true);
                var imageQWC = encodedImage.Data.Length / 16;

                clutAllocList.Add(clutBuilder.Allocate(encodedImage.PixelFormat == PixelFormat.Indexed4));

                var source = userTextureTransfer[imageIndex];

                texTransList.Add(
                    new DataTransferInfo
                    {
                        BitBltBuf = new BITBLTBUF(source.BitBltBuf.Data)
                        {
                            DBP = TexBasePtr,
                        },
                        TrxPos = source.TrxPos,
                        TrxReg = source.TrxReg,
                        TrxDir = source.TrxDir,

                        QuadWordCount = imageQWC,
                        DataOffset = Convert.ToInt32(picData.Position), // relative
                    }
                );

                picData.Write(encodedImage.Data);
            }

            foreach (var (imageIndex, gsIndex) in OffsetData.Select((imageIndex, gsIndex) => (imageIndex, gsIndex)))
            {
                var clutAlloc = clutAllocList[imageIndex];
                var image = build.images[imageIndex];

                var source = userGsInfo[gsIndex];

                var tex2 = new Tm2.GsTex();
                tex2.Data = source.Tex2.Data;
                tex2.PSM = Tm2.GsPSM.GS_PSMT8; // PSMT8 will load 256 clut entries starts at CBP
                tex2.CBP = clutAlloc.CBP;
                tex2.CPSM = Tm2.GsCPSM.GS_PSMCT32; // RGBA palette
                tex2.CSM = false;
                tex2.CSA = 0; // always 0
                tex2.CLD = 4;

                var tex0 = new Tm2.GsTex();
                tex0.Data = source.Tex0.Data;
                tex0.TBP0 = TexBasePtr;
                tex0.CBP = clutAlloc.CBP;
                tex0.CPSM = Tm2.GsCPSM.GS_PSMCT32; // RGBA palette
                tex0.CSM = false;
                tex0.CSA = clutAlloc.CSA;

                gsInfoList.Add(
                    new GsInfo
                    {
                        MipTbp1 = source.MipTbp1,
                        MipTbp2 = source.MipTbp2,
                        Tex2 = tex2,
                        Tex1 = source.Tex1,
                        Tex0 = tex0,
                        AddressMode = source.AddressMode,
                    }
                );

                clutAlloc.Write(palData, image.GetClut());
            }

            PictureData = picData.ToArray();
            PaletteData = palData.ToArray();
            FooterData = Encoding.ASCII.GetBytes("_KN5");

            var clutQWC = Convert.ToInt32(palData.Length / 16);

            _clutTransfer = new DataTransferInfo
            {
                BitBltBuf = new BITBLTBUF
                {
                    DBP = ClutBasePtr,
                    DBW = 1, // clut is always width=64
                    DPSM = (int)Tm2.GsPSM.GS_PSMCT32,
                },
                TrxPos = new TRXPOS
                {

                },
                TrxReg = new TRXREG
                {
                    RRW = 64,
                    RRH = clutQWC / 16,
                },
                TrxDir = new TRXDIR
                {

                },
                QuadWordCount = clutQWC,
            };
            _textureTransfer = texTransList;
            _gsInfo = gsInfoList;
        }

        private static int GetSizeRegister(int realSize) => (int)Math.Ceiling(Math.Log(realSize, 2));

        private static Tm2.GsPSM ToPSM(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Indexed4: return Tm2.GsPSM.GS_PSMT4;
                case PixelFormat.Indexed8: return Tm2.GsPSM.GS_PSMT8;
            }
            throw new NotSupportedException();
        }

        public void Write(Stream stream)
        {
            stream.Position = HeaderLength;
            stream.Write(OffsetData);
            stream.AlignPosition(0x10);

            var texInfo1Offset = (int)stream.Position;

            var dataTransferList = new DataTransferInfo[] { _clutTransfer }
                .Concat(_textureTransfer)
                .ToArray();

            foreach (var dataTransfer in dataTransferList)
            {
                BinaryMapping.WriteObject(stream, new _DataTransfer
                {
                    Data00 = 0x10000006, // dmaSourceChainTag: id=cnt (continue), qwc=6
                    Data04 = 0x00000000, // dmaSourceChainTag: addr=0x00000000
                    Data08 = 0x13000000, // VIFcode1: FLUSHA
                    Data0c = 0x50000006, // VIFcode2: DIRECT, subPacketLength=6
                    Data10 = 0x00000004, // GIFtag: nloop=4, eop=0,
                    Data14 = 0x10000000, // GIFtag: flg=packed, nreg=1
                    Data18 = 0x0000000e, // GIFtag: registerDesc[0]=A+D
                    Data1c = 0x00000000, // GIFtag: 
                    BitBltBuf = dataTransfer.BitBltBuf,
                    GSReg50 = 0x0000000000000050, // GS register: 0x50 BITBLTBUF
                    TrxPos = dataTransfer.TrxPos,
                    GSReg51 = 0x0000000000000051, // GS register: 0x51 TRXPOS
                    TrxReg = dataTransfer.TrxReg,
                    GSReg52 = 0x0000000000000052, // GS register: 0x52 TRXREG
                    TrxDir = dataTransfer.TrxDir,
                    GSReg53 = 0x0000000000000053, // GS register: 0x53 TRXDIR
                    Data60 = 0x00008000 | dataTransfer.QuadWordCount, // GIFtag: nloop=qwc, eop=1
                    Data64 = 0x08000000, // GIFtag: flg=image
                    Data68 = 0x00000000, // GIFtag: 
                    Data6c = 0x00000000, // GIFtag: 
                    Data70 = 0x30000000 | dataTransfer.QuadWordCount, // dmaSourceChainTag: id=ref, qwc=qwc
                    DataOffset = dataTransfer.DataOffset, // dmaSourceChainTag: addr=it
                    Data78 = 0x00000000, // VIFcode1: NOP
                    Data7c = 0x50000000 | dataTransfer.QuadWordCount, // VIFcode2: DIRECT
                    Data80 = 0x60000000, // dmaSourceChainTag: id=ret, qwc=0
                    Data84 = 0x00000000, // dmaSourceChainTag: addr=0
                    Data88 = 0x13000000, // VIFcode1: FLUSHA
                    Data8c = 0x00000000, // VIFcode2: NOP
                });
            }

            var texInfo2Offset = (int)stream.Position;
            foreach (var textureInfo in _gsInfo)
                BinaryMapping.WriteObject(stream, new _GsInfo
                {
                    Data00 = 0x0000000010000008, // dmaSourceChainTag: id=cnt (continue), qwc=8
                    Data08 = 0x5000000813000000, // VIFcode1: FLUSHA // VIFcode2: DIRECT, subPacketLength=8
                    Data10 = 0x1000000000008007, // GIFtag: nloop=7, eop=1, flg=packed, nreg=1
                    Data18 = 0x000000000000000e, // GIFtag: registerDesc[0]=A+D
                    TexFlush = 0x0000000000000000,
                    GSReg3F = 0x000000000000003f, // GS register: TEXFLUSH
                    MipTbp1 = textureInfo.MipTbp1,
                    GSReg34 = 0x0000000000000034, // GS register: MIPTBP1_1
                    MipTbp2 = textureInfo.MipTbp2,
                    GSReg36 = 0x0000000000000036, // GS register: MIPTBP2_1
                    Tex2 = textureInfo.Tex2,
                    GSReg16 = 0x0000000000000016, // GS register: TEX2_1
                    Tex1 = textureInfo.Tex1,
                    GSReg14 = 0x0000000000000014, // GS register: TEX1_1
                    Tex0 = textureInfo.Tex0,
                    GSReg06 = 0x0000000000000006, // GS register: TEX0_1
                    Clamp = new CLAMP(ClampToGs(textureInfo.AddressMode)),
                    Data88 = 0x0000000000000008, // GS register: CLAMP_1
                    Data90 = 0x0000000060000000, // dmaSourceChainTag: id=ret (continue), qwc=0
                    Data98 = 0x0000000013000000, // VIFcode1: FLUSHA // VIFcode21: NOP
                });

            stream.AlignPosition(0x80);

            var pictureOffset = (int)stream.Position;
            stream.Write(PictureData);

            var paletteOffset = (int)stream.Position;
            stream.Write(PaletteData);

            stream.Write(FooterData);

            var writer = new BinaryWriter(stream.SetPosition(0));
            writer.Write(MagicCode);
            writer.Write(PaletteData.Length / 4);
            writer.Write(_textureTransfer.Count);
            writer.Write(_gsInfo.Count);
            writer.Write(HeaderLength);
            writer.Write(texInfo1Offset);
            writer.Write(texInfo2Offset);
            writer.Write(pictureOffset);
            writer.Write(paletteOffset);

            if (_useRelativeOffset)
            {
                {
                    AddOffset(stream, texInfo1Offset + 0x74, paletteOffset);
                }
                for (var index = 0; index < _textureTransfer.Count; index++)
                {
                    AddOffset(stream, texInfo1Offset + 0x90 * (1 + index) + 0x74, pictureOffset);
                }

                stream.Seek(0, SeekOrigin.End);
            }
        }

        private static void AddOffset(Stream stream, int fileOffset, int delta)
        {
            var buff = new byte[4];

            stream.Position = fileOffset;
            stream.Read(buff, 0, 4);

            var val = (int)buff[0];
            val |= (buff[1] << 8);
            val |= (buff[2] << 16);
            val |= (buff[3] << 24);

            val += delta;

            buff[0] = (byte)(val);
            buff[1] = (byte)(val >> 8);
            buff[2] = (byte)(val >> 16);
            buff[3] = (byte)(val >> 24);

            stream.Position = fileOffset;
            stream.Write(buff, 0, 4);
        }

        private TextureAddressMode ClampFromGs(long data)
        {
            return new TextureAddressMode
            {
                AddressU = (TextureWrapMode)GetBits(data, 0, 2), // WMS
                AddressV = (TextureWrapMode)GetBits(data, 2, 2), // WMT
                Left = GetBits(data, 4, 10), // MINU
                Right = GetBits(data, 14, 10), // MAXU
                Top = GetBits(data, 24, 10), // MINV
                Bottom = GetBits(data, 34, 10), // MAXV
            };
        }

        private long ClampToGs(TextureAddressMode addressMode)
        {
            long value = 0;
            value = SetBits(value, 0, 2, (int)addressMode.AddressU); // WMS
            value = SetBits(value, 2, 2, (int)addressMode.AddressV); // WMT
            value = SetBits(value, 4, 10, addressMode.Left); // MINU
            value = SetBits(value, 14, 10, addressMode.Right); // MAXU
            value = SetBits(value, 24, 10, addressMode.Top); // MINV
            value = SetBits(value, 34, 10, addressMode.Bottom); // MAXV
            return value;
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

        private static byte[] GetClut4(byte[] clut, int cbp, int csa)
        {
            var data = new byte[16 * 4];
            for (var i = 0; i < 16; i++)
            {
                var srcIndex = GetClutPointer(i, cbp, csa);

                data[i * 4 + 0] = clut[srcIndex * 4 + 0];
                data[i * 4 + 1] = clut[srcIndex * 4 + 1];
                data[i * 4 + 2] = clut[srcIndex * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(clut[srcIndex * 4 + 3]);
            }

            return data;
        }

        private static byte[] GetClut8(byte[] clut, int cbp, int csa)
        {
            var data = new byte[256 * 4];
            for (var i = 0; i < 256; i++)
            {
                var srcIndex = GetClutPointer(i, cbp, csa);

                data[i * 4 + 0] = clut[srcIndex * 4 + 0];
                data[i * 4 + 1] = clut[srcIndex * 4 + 1];
                data[i * 4 + 2] = clut[srcIndex * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(clut[srcIndex * 4 + 3]);
            }

            return data;
        }

        public static ModelTexture Read(Stream stream) =>
            new ModelTexture(stream);

        public static bool IsValid(Stream stream)
        {
            if (stream.Length < HeaderLength)
                return false;

            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));
            if (header.MagicCode != MagicCode)
                return false;

            var streamLength = stream.Length;
            if (header.Offset1 > streamLength ||
                header.Texinf1off > streamLength ||
                header.Texinf2off > streamLength ||
                header.PictureOffset > streamLength ||
                header.PaletteOffset > streamLength ||
                header.TextureInfoCount <= 0 ||
                header.GsInfoCount <= 0)
                return false;

            return true;
        }

        public static int GetClutPointer(int index, int cbp, int csa)
        {
            return (index & 7) + (index & 8) * 8 + (index & 16) / 2 + (index & ~31) * 4 +
                (cbp & 7) * 0x4 + (cbp & 8) * 0x80 + (cbp & 16) * 0x2 + +(cbp & ~31) * 0x40 +
                (csa & 1) * 0x8 + (csa & 14) * 0x40;
        }

        private static int GetBits(long Data, int position, int size)
        {
            var mask = (1 << size) - 1;
            return (int)((Data >> position) & mask);
        }

        private static long SetBits(long Data, int position, int size, int value)
        {
            var mask = (1 << size) - 1U;
            return Data & ~(mask << position) | ((value & mask) << position);
        }
    }
}
