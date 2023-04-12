using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Txa
    {
        public class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public ushort Version { get; set; }
            [Data] public ushort GroupCount { get; set; }
            [Data] public uint Padding0 { get; set; }
            [Data] public uint Padding1 { get; set; }
        }

        public class Group
        {
            [Data(Count = 16)] public string Name { get; set; }
            [Data(Count = 24)] public string DestTexName { get; set; }
            [Data] public uint AlwaysZero { get; set; } // Ptr to engine structure, always zero
            [Data] public short DestWidth { get; set; }
            [Data] public short DestHeight { get; set; }
            [Data] public short AnimCount { get; set; }
            [Data] public short DefaultAnim { get; set; }
            [Data] public uint OffsetAnims { get; set; }

            public Anim[] Anims { get; set; }
        }

        public class Anim
        {
            [Data(Count = 16)] public string Name { get; set; }
            [Data] public short UnkNum { get; set; }    // I think this is how many frames to jump back once we reach the end of the anim. Seems to always be FrameCount * -1.
            [Data] public short FrameCount { get; set; }
            [Data] public uint OffsetFrames { get; set; }

            public Frame[] Frames { get; set; }
        }

        public class Frame
        {
            [Data] public uint OffsetPixels { get; set; }
            [Data] public short UnkNum1 { get; set; }   // These 2 are something to do with how long the frame is displayed for
            [Data] public short UnkNum2 { get; set; }
            [Data] public sbyte UnkByte1 { get; set; }  // Used by engine, probably zero?
            [Data] public byte UnkByte2 { get; set; }   // Used by engine, probably zero?
            [Data] public short Padding { get; set; }

            public byte[] Pixels { get; set; }
            public SimpleImage Image { get; set; }
        }

        public List<Group> AnimGroups { get; set; }

        public static Txa Read(Stream stream, Pmo model) => new Txa(stream.SetPosition(0), model);

        public Txa(Stream stream, Pmo model)
        {
            Header header = BinaryMapping.ReadObject<Header>(stream);
            AnimGroups = Enumerable.Range(0, header.GroupCount)
                            .Select(i => BinaryMapping.ReadObject<Group>(stream))
                            .ToList();

            foreach (var group in AnimGroups)
            {
                stream.SetPosition(group.OffsetAnims);
                group.Anims = Enumerable.Range(0, group.AnimCount).Select(i => BinaryMapping.ReadObject<Anim>(stream)).ToArray();
                foreach (var anim in group.Anims)
                {
                    stream.SetPosition(anim.OffsetFrames);
                    anim.Frames = Enumerable.Range(0, anim.FrameCount).Select(i => BinaryMapping.ReadObject<Frame>(stream)).ToArray();
                    foreach (var frame in anim.Frames)
                    {
                        if (frame.OffsetPixels == 0)
                            continue;

                        var dstTexIdx = Array.FindIndex(model.textureInfo, ti => ti.TextureName == group.DestTexName);
                        var dstTex = model.texturesData[dstTexIdx];
                        int szMul;
                        switch (dstTex.PixelFormat)
                        {
                            default:
                            case Imaging.PixelFormat.Undefined:
                                throw new Exception("WTF?");
                            case Imaging.PixelFormat.Indexed4:
                            case Imaging.PixelFormat.Indexed8:
                                szMul = 1;
                                break;
                            case Imaging.PixelFormat.Rgba1555:
                                szMul = 2;
                                break;
                            case Imaging.PixelFormat.Rgb888:
                            case Imaging.PixelFormat.Rgbx8888:
                            case Imaging.PixelFormat.Rgba8888:
                                szMul = 4;
                                break;
                        }
                        stream.SetPosition(frame.OffsetPixels);
                        frame.Pixels = stream.ReadBytes(szMul * group.DestWidth * group.DestHeight);
                        frame.Image = new SimpleImage(group.DestWidth, group.DestHeight, dstTex.PixelFormat, frame.Pixels, dstTex.ClutFormat, dstTex.GetClut());
                    }
                }
            }
        }
    }
}
