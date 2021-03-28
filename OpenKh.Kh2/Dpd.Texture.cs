using System;
using System.Drawing;
using System.IO;

namespace OpenKh.Kh2
{
    public partial class Dpd
    {
        public class Texture
        {
            private short unk00;
            private short unk02;
            private short unk04;
            private short format;
            private short unk08;
            private short unk0a;
            private short width;
            private short height;
            private short unk10;
            private short unk12;
            private short unk14;
            private short unk16;
            private int unk18;
            private int unk1c;

            public Texture()
            {

            }

            internal Texture(BinaryReader reader)
            {
                var basePosition = reader.BaseStream.Position;
                unk00 = reader.ReadInt16();
                unk02 = reader.ReadInt16();
                unk04 = reader.ReadInt16();
                format = reader.ReadInt16();
                unk08 = reader.ReadInt16();
                unk0a = reader.ReadInt16();
                width = reader.ReadInt16();
                height = reader.ReadInt16();
                unk10 = reader.ReadInt16();
                unk12 = reader.ReadInt16();
                unk14 = reader.ReadInt16();
                unk16 = reader.ReadInt16();
                unk18 = reader.ReadInt32();
                unk1c = reader.ReadInt32();

                Data = reader.ReadBytes(width * height);
                Palette = reader.ReadBytes(0x100 * sizeof(int));
            }

            internal void Write(BinaryWriter writer)
            {
                writer.Write(unk00);
                writer.Write(unk02);
                writer.Write(unk04);
                writer.Write(format);
                writer.Write(unk08);
                writer.Write(unk0a);
                writer.Write(width);
                writer.Write(height);
                writer.Write(unk10);
                writer.Write(unk12);
                writer.Write(unk14);
                writer.Write(unk16);
                writer.Write(unk18);
                writer.Write(unk1c);

                writer.Write(Data);
                writer.Write(Palette);
            }

            public Size Size => new Size(width, height);

            public byte[] Data { get; }

            public byte[] Palette { get; }

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
