using OpenKh.Common;
using OpenKh.Command.MapGen.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xe.BinaryMapper;
using OpenKh.Kh2;
using System.Linq;
using McMaster.Extensions.CommandLineUtils.Conventions;
using System.ComponentModel;
using System.Numerics;

namespace OpenKh.Command.MapGen.Utils
{
    class MapVifPacketBuilder
    {
        public MemoryStream vifPacket = new MemoryStream();

        public ushort firstVifPacketQwc;

        public class Index
        {
            /// <summary>
            /// 0x10, 0x10, 0x20, 0x30, 0x20, 0x30, ...
            /// </summary>
            public byte Flag { get; set; }

            /// <summary>
            /// Index to coords
            /// </summary>
            public byte CoordIndex { get; set; }

            /// <summary>
            /// 0.0 to 1.0
            /// </summary>
            public Vector2 UV { get; set; }

            /// <summary>
            /// vertex color rgba is directly written.
            /// 128 is PS2's max. 255 is doubled intensity.
            /// </summary>
            public Xe.Graphics.Color Color { get; set; }

            /// <summary>
            /// Normal vector
            /// </summary>
            public Vector3 Normal { get; set; }

            public static byte GetSuitableFlag(int index)
            {
                switch (index)
                {
                    case 0:
                    case 1:
                        return 0x10;
                    default:
                        return ((index & 1) == 0) ? (byte)0x20 : (byte)0x30;
                }
            }
        }

        public MapVifPacketBuilder(
            IEnumerable<Vector3> coords,
            IEnumerable<Index> indices,
            bool useNormal
        )
        {
            var lenLarge = indices.Count();
            var lenSmall = coords.Count();
            var lenNormal = useNormal ? lenLarge : 0;
            var lenHeader = useNormal ? 5 : 4;

            var adder = new OffsetAdder();

            var offHeader = adder.Add(lenHeader);
            var offVertIdx = adder.Add(lenLarge); // u v idx flags
            var offClrVert = adder.Add(lenLarge); // r g b a
            var offNormal = adder.Add(lenNormal); // x y z
            var offPosVert = adder.Add(lenSmall); // x y z

            var top = new TopHeader
            {
                numVertIdx = lenLarge,
                offVertIdx = offVertIdx,

                numClrVert = lenLarge,
                offClrVert = offClrVert,

                numPosVert = lenSmall,
                offPosVert = offPosVert,
            };
            var top2 = new TopHeader2
            {
                numNormal = lenNormal,
                offNormal = offNormal,
            };

            {
                var writer = new BinaryWriter(vifPacket);

                {
                    var num = lenHeader;

                    writer.Write(0x01000101); // stcycl cl 01 wl 01
                    writer.Write(0x6C008000 | (num << 16)); // unpack V4-32 c 4 a 000 usn 0 flg 1 m 0
                    BinaryMapping.WriteObject(vifPacket, top);
                    if (useNormal)
                    {
                        BinaryMapping.WriteObject(vifPacket, top2);
                    }
                }

                vifPacket.AlignPosition(4);

                {
                    // write uv
                    var off = offVertIdx;
                    var num = Convert.ToByte(lenLarge);
                    writer.Write(0x01000101); // stcycl cl 01 wl 01
                    writer.Write((int)(0x65008000 | off | (num << 16))); // unpack V2-16 c 14 a 004 usn 0 flg 1 m 0
                    foreach (var one in indices)
                    {
                        writer.Write(Convert.ToInt16(Math.Max(-32768, Math.Min(32767, (one.UV.X * 4096)))));
                        writer.Write(Convert.ToInt16(Math.Max(-32768, Math.Min(32767, (one.UV.Y * 4096)))));
                    }
                }

                vifPacket.AlignPosition(4);

                {
                    // write idx
                    var off = offVertIdx;
                    var num = Convert.ToByte(lenLarge);
                    writer.Write(0x20000000); // stmask  3 3 0 3  3 3 0 3  3 3 0 3  3 3 0 3 
                    writer.Write(0xcfcfcfcf);
                    writer.Write(0x01000101); // stcycl cl 01 wl 01
                    writer.Write((int)(0x7200C000 | off | (num << 16))); // unpack S-8 c 14 a 004 usn 1 flg 1 m 1
                    foreach (var one in indices)
                    {
                        writer.Write(Convert.ToByte(one.CoordIndex));
                    }
                }

                vifPacket.AlignPosition(4);

                {
                    // write flags
                    var off = offVertIdx;
                    var num = Convert.ToByte(lenLarge);
                    writer.Write(0x20000000); // stmask  3 3 3 0  3 3 3 0  3 3 3 0  3 3 3 0 
                    writer.Write(0x3f3f3f3f);
                    writer.Write(0x01000101); // stcycl cl 01 wl 01
                    writer.Write((int)(0x7200C000 | off | (num << 16))); // unpack S-8 c 14 a 004 usn 1 flg 1 m 1

                    foreach (var one in indices)
                    {
                        writer.Write(Convert.ToByte(one.Flag));
                    }
                }

                vifPacket.AlignPosition(4);

                {
                    // write vertex color list
                    var off = offClrVert;
                    var num = Convert.ToByte(lenLarge);
                    writer.Write(0x01000101); // stcycl cl 01 wl 01
                    writer.Write((int)(0x6E00C000 | off | (num << 16))); // unpack V4-8 c 14 a 012 usn 1 flg 1 m 0
                    foreach (var color in indices.Select(it => it.Color))
                    {
                        writer.Write(color.r);
                        writer.Write(color.g);
                        writer.Write(color.b);
                        writer.Write(color.a);
                    }
                }

                vifPacket.AlignPosition(4);

                if (useNormal)
                {
                    var off = offNormal;
                    var num = Convert.ToByte(top2.numNormal);

                    {
                        writer.Write(0x20000000); // stmask  0 0 0 3  0 0 0 3  0 0 0 3  0 0 0 3 
                        writer.Write(0xC0C0C0C0);
                        writer.Write(0x01000101); // stcycl cl 01 wl 01
                        writer.Write((int)(0x78008000 | off | (num << 16))); // unpack V3-32 c 9 a 020 usn 0 flg 1 m 1
                        foreach (var one in indices)
                        {
                            writer.Write(one.Normal.X);
                            writer.Write(one.Normal.X);
                            writer.Write(one.Normal.Z);
                        }
                    }

                    {
                        writer.Write(0x20000000); // stmask  3 3 3 0  3 3 3 0  3 3 3 0  3 3 3 0 
                        writer.Write(0x3f3f3f3f);
                        writer.Write(0x01000101); // stcycl cl 01 wl 01
                        writer.Write((int)(0x7200C000 | off | (num << 16))); // unpack S-8 c 14 a 004 usn 1 flg 1 m 1
                        foreach (var one in indices)
                        {
                            writer.Write(Convert.ToByte(0x00));
                        }
                    }
                }

                vifPacket.AlignPosition(4);

                {
                    // write vertices
                    var off = offPosVert;
                    var num = Convert.ToByte(lenSmall);
                    writer.Write(0x31000000); // stcol 3f800000 3f800000 3f800000 3f800000
                    writer.Write(0x3f800000);
                    writer.Write(0x3f800000);
                    writer.Write(0x3f800000);
                    writer.Write(0x3f800000);
                    writer.Write(0x20000000); // stmask  0 0 0 2  0 0 0 2  0 0 0 2  0 0 0 2 
                    writer.Write(0x80808080);
                    writer.Write(0x01000101); // stcycl cl 01 wl 01
                    writer.Write((int)(0x78008000 | off | (num << 16))); // unpack V3-32 c 9 a 020 usn 0 flg 1 m 1
                    foreach (var one in coords)
                    {
                        writer.Write(one.X);
                        writer.Write(one.Y);
                        writer.Write(one.Z);
                    }
                }

                vifPacket.AlignPosition(4);

                writer.Write(0x17000000); // mscnt
            }

            Align16Plus8(vifPacket);

            firstVifPacketQwc = Convert.ToUInt16(vifPacket.Length / 16);

            vifPacket.Write(new byte[8], 0, 8); // write later 8 bytes of the follwing dma (id=RET) packet.
        }

        private void Align16Plus8(Stream stream)
        {
            stream.Position = stream.Length;
            var fill = new byte[(16 - ((int)(stream.Length - 8) & 15)) & 15];
            stream.Write(fill, 0, fill.Length);
        }

        private static byte ToPs2Alpha(byte data) => (byte)((data + 1) / 2);

        private class OffsetAdder
        {
            private int _offset = 0;

            public int Add(int len)
            {
                var thisOffset = _offset;
                _offset += len;
                return thisOffset;
            }
        }

        private class DmaSourceChainTag
        {
            [Data] public ushort qwc { get; set; }
            [Data] public ushort idFlg { get; set; }
            [Data] public uint addr { get; set; }
        }

        /// <summary>
        /// VPU1 map program header
        /// </summary>
        private class TopHeader
        {
            [Data] public int unk1 { get; set; }
            [Data] public int unk2 { get; set; }
            [Data] public int unk3 { get; set; }
            [Data] public int unk4 { get; set; }

            [Data] public int numVertIdx { get; set; }
            [Data] public int offVertIdx { get; set; }
            [Data] public int unk5 { get; set; }
            [Data] public int unk6 { get; set; }

            [Data] public int numClrVert { get; set; }
            [Data] public int offClrVert { get; set; }
            [Data] public int unk7 { get; set; }
            [Data] public int unk8 { get; set; }

            [Data] public int numPosVert { get; set; }
            [Data] public int offPosVert { get; set; }
            [Data] public int unk9 { get; set; }
            [Data] public int unk10 { get; set; }
        }

        /// <summary>
        /// TopHeader + normal
        /// </summary>
        private class TopHeader2
        {
            [Data] public int numNormal { get; set; }
            [Data] public int offNormal { get; set; }
            [Data] public int unk1 { get; set; }
            [Data] public int unk2 { get; set; }
        }
    }
}
