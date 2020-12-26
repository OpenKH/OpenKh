// Inspired by Kddf2's khkh_xldM.
// Original source code: https://gitlab.com/kenjiuno/khkh_xldM/blob/master/khkh_xldMii/Mdlxfst.cs

using OpenKh.Common;
using OpenKh.Common.Ps2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using Xe.IO;

namespace OpenKh.Kh2
{
    public partial class Mdlx
    {
        private static readonly VifCode DefaultVifCode = new VifCode
        {
            Opcode = VifOpcode.STCYCL,
            Interrupt = false,
            Num = 1,
            Immediate = 0x0100,
        };
        private static readonly DmaPacket CloseDmaPacket = new DmaPacket
        {
            DmaTag = new DmaTag
            {
                Qwc = 0,
                TagId = 1,
                Irq = false,
                Address = 0
            },
            VifCode = new VifCode
            {
                Opcode = VifOpcode.NOP,
                Interrupt = false,
                Num = 0,
                Immediate = 0x1700,
            },
            Parameter = 0
        };

        public class Bone
        {
            [Data] public int Index { get; set; }
            [Data] public int Parent { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0c { get; set; }
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public float ScaleW { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public float RotationW { get; set; }
            [Data] public float TranslationX { get; set; }
            [Data] public float TranslationY { get; set; }
            [Data] public float TranslationZ { get; set; }
            [Data] public float TranslationW { get; set; }
        }

        public class DmaPacket
        {
            [Data] public DmaTag DmaTag { get; set; }
            [Data] public VifCode VifCode { get; set; }
            [Data] public int Parameter { get; set; }
        }

        private class SubModelHeader
        {
            [Data] public int Type { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int NextOffset { get; set; }
            [Data] public short BoneCount { get; set; }
            [Data] public short Unk { get; set; }
            [Data] public int BoneOffset { get; set; }
            [Data] public int UnkDataOffset { get; set; }
            [Data] public int DmaChainCount { get; set; }
        }

        private class DmaChainHeader
        {
            [Data] public int Unk00 { get; set; }
            [Data] public int TextureIndex { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unused1 { get; set; }
            [Data] public int DmaOffset { get; set; }
            [Data] public int Count1aOffset { get; set; }
            [Data] public int DmaLength { get; set; }
            [Data] public int Unused2 { get; set; }
        }

        public class SubModel
        {
            public int Type { get; set; }
            public int Unk04 { get; set; }
            public int Unk08 { get; set; }
            public short BoneCount { get; set; }
            public short Unk { get; set; }
            public int DmaChainCount { get; set; }

            public List<Bone> Bones { get; internal set; }
            public byte[] UnknownData { get; internal set; }
            public List<DmaChain> DmaChains { get; internal set; }
        }

        public class DmaChain
        {
            public int RenderFlags { get; set; }
            public int TextureIndex { get; set; }
            public int Unk08 { get; set; }
            public int DmaLength { get; set; }
            public List<DmaVif> DmaVifs { get; set; }
        }

        public class DmaVif
        {
            public int TextureIndex { get; }
            public int[] Alaxi { get; }
            public byte[] VifPacket { get; }
            public int BaseAddress { get; }

            public DmaVif(int texi, int[] alaxi, byte[] bin, int baseAddress)
            {
                TextureIndex = texi;
                Alaxi = alaxi;
                VifPacket = bin;
                BaseAddress = baseAddress;
            }
        }

        private static IEnumerable<SubModel> ReadAsModel(Stream stream)
        {
            var currentOffset = 0;
            var nextOffset = ReservedArea;
            while (nextOffset != 0)
            {
                currentOffset += nextOffset;
                var subStream = new SubStream(stream, currentOffset, stream.Length - currentOffset);
                if (subStream.Length == 0)
                    yield break;

                nextOffset = ReadSubModel(subStream, out var subModel);

                yield return subModel;
            }
        }

        private static int ReadSubModel(Stream stream, out SubModel subModel)
        {
            var header = BinaryMapping.ReadObject<SubModelHeader>(stream);
            subModel = new SubModel
            {
                Type = header.Type,
                Unk04 = header.Unk04,
                Unk08 = header.Unk08,
                BoneCount = header.BoneCount,
                Unk = header.Unk,
                DmaChainCount = header.DmaChainCount,
            };
            
            var dmaChainHeaders = For(subModel.DmaChainCount, () => BinaryMapping.ReadObject<DmaChainHeader>(stream));

            stream.Position = header.UnkDataOffset;
            subModel.UnknownData = stream.ReadBytes(0x120);

            if (header.BoneOffset != 0)
            {
                stream.Position = header.BoneOffset;
                subModel.Bones = For(subModel.BoneCount, () => ReadBone(stream)).ToList();
            }

            subModel.DmaChains = dmaChainHeaders.Select(x => ReadDmaChain(stream, x)).ToList();

            return header.NextOffset;
        }

        private static DmaChain ReadDmaChain(Stream stream, DmaChainHeader dmaChainHeader)
        {
            var dmaVifs = new List<DmaVif>();
            var count1a = stream.SetPosition(dmaChainHeader.Count1aOffset).ReadInt32();
            var alv1 = For(count1a, () => stream.ReadInt32()).ToList();

            var offsetDmaPackets = new List<int>();
            var alaxi = new List<int[]>();
            var alaxref = new List<int>();

            offsetDmaPackets.Add(dmaChainHeader.DmaOffset);

            var offsetDmaBase = dmaChainHeader.DmaOffset + 0x10;

            for (var i = 0; i < count1a; i++)
            {
                if (alv1[i] == -1)
                {
                    offsetDmaPackets.Add(offsetDmaBase + 0x10);
                    offsetDmaBase += 0x20;

                    alaxi.Add(alaxref.ToArray());
                    alaxref.Clear();
                }
                else
                {
                    offsetDmaBase += 0x10;
                    alaxref.Add(alv1[i]);
                }
            }

            alaxi.Add(alaxref.ToArray());
            alaxref.Clear();

            var dmaPackets = offsetDmaPackets
                .Select(offset => ReadTags(stream.SetPosition(offset)).ToArray())
                .ToArray();

            for (var i = 0; i < offsetDmaPackets.Count; i++)
            {
                var dmaTag = dmaPackets[i][0].DmaTag;
                var baseAddress = dmaTag.Qwc != 0 ? dmaPackets[i][1].Parameter : 0;
                stream.Position = dmaTag.Address & 0x7FFFFFFF;
                var vifPacket = stream.ReadBytes(dmaTag.Qwc * 0x10);
                dmaVifs.Add(new DmaVif(dmaChainHeader.TextureIndex, alaxi[i], vifPacket, baseAddress));
            }

            return new DmaChain
            {
                RenderFlags = dmaChainHeader.Unk00,
                TextureIndex = dmaChainHeader.TextureIndex,
                Unk08 = dmaChainHeader.Unk08,
                DmaLength = dmaChainHeader.DmaLength,
                DmaVifs = dmaVifs,
            };
        }

        private static IEnumerable<DmaPacket> ReadTags(Stream stream)
        {
            while (true)
            {
                var dmaPacket = BinaryMapping.ReadObject<DmaPacket>(stream);
                yield return dmaPacket;

                if (dmaPacket.DmaTag.Qwc == 0)
                    yield break;
            }
        }

        private static void WriteSubModel(Stream stream, SubModel subModel, int baseAddress)
        {
            var header = new SubModelHeader
            {
                Type = subModel.Type,
                Unk04 = subModel.Unk04,
                Unk08 = subModel.Unk08,
                Unk = subModel.Unk,
                DmaChainCount = subModel.DmaChains.Count,
            };

            stream.Position += 0x20; // skip header
            stream.Position += subModel.DmaChainCount * 0x20;

            if (subModel.Type == Entity)
            {
                header.UnkDataOffset = (int)stream.Position;
                stream.Write(subModel.UnknownData);

                header.BoneOffset = (int)stream.Position;
                header.BoneCount = (short)subModel.Bones.Count;
                foreach (var bone in subModel.Bones)
                    WriteBone(stream, bone);
            }
            else if (subModel.Type == Shadow)
            {
                header.UnkDataOffset = 0;
                header.BoneOffset = 0;
                header.BoneCount = subModel.BoneCount;
            }
            else
                throw new NotImplementedException($"Submodel type {subModel.Type} not supported.");

            var dmaChainHeaders = subModel.DmaChains.Select(x => WriteDmaChain(stream, x)).ToList();

            stream.SetLength(stream.AlignPosition(0x80).Position);
            header.NextOffset = baseAddress >= 0 ? (int)(baseAddress + stream.Position) : 0;

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, header);
            foreach (var dmaChainHeader in dmaChainHeaders)
                BinaryMapping.WriteObject(stream, dmaChainHeader);
        }

        private static DmaChainHeader WriteDmaChain(Stream stream, DmaChain dmaChain)
        {
            var dmaChainHeader = new DmaChainHeader
            {
                Unk00 = dmaChain.RenderFlags,
                TextureIndex = dmaChain.TextureIndex,
                Unk08 = dmaChain.Unk08,
                DmaLength = dmaChain.DmaLength,
            };

            var dmaVifs = dmaChain.DmaVifs;

            var vifPacketOffsets = new List<int>();
            foreach (var dmaVif in dmaVifs)
            {
                vifPacketOffsets.Add((int)stream.Position);
                stream.Write(dmaVif.VifPacket);
            }

            dmaChainHeader.DmaOffset = (int)stream.Position;

            for (var i = 0; i < dmaVifs.Count; i++)
            {
                if (dmaVifs[i].BaseAddress > 0)
                {
                    BinaryMapping.WriteObject(stream, new DmaPacket
                    {
                        DmaTag = new DmaTag
                        {
                            Qwc = (ushort)(dmaVifs[i].VifPacket.Length / 0x10),
                            TagId = 3,
                            Irq = false,
                            Address = vifPacketOffsets[i]
                        },
                        VifCode = new VifCode { },
                        Parameter = 0
                    });

                    for (var j = 0; j < dmaVifs[i].Alaxi.Length; j++)
                    {
                        BinaryMapping.WriteObject(stream, new DmaPacket
                        {
                            DmaTag = new DmaTag
                            {
                                Qwc = 4,
                                TagId = 3,
                                Irq = false,
                                Address = dmaVifs[i].Alaxi[j]
                            },
                            VifCode = DefaultVifCode,
                            Parameter = dmaVifs[i].BaseAddress + j * 4
                        });
                    }
                }

                BinaryMapping.WriteObject(stream, CloseDmaPacket);
            }

            var alv1 = new List<int>();
            foreach (var vif in dmaVifs)
            {
                alv1.AddRange(vif.Alaxi);
                alv1.Add(-1);
            }
            alv1.RemoveAt(alv1.Count - 1);

            dmaChainHeader.Count1aOffset = (int)stream.Position;
            stream.Write(alv1.Count);
            foreach (var alvItem in alv1)
                stream.Write(alvItem);

            stream.AlignPosition(0x10);

            return dmaChainHeader;
        }

        private static Bone ReadBone(Stream stream) => BinaryMapping.ReadObject<Bone>(stream);
        private static void WriteBone(Stream stream, Bone bone) => BinaryMapping.WriteObject(stream, bone);
    }
}
