// Inspired by Kddf2's khkh_xldM.
// Original source code: https://gitlab.com/kenjiuno/khkh_xldM/blob/master/khkh_xldMii/Mdlxfst.cs

using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using Xe.IO;

namespace OpenKh.Kh2
{
    public partial class Mdlx
    {
        public class Bone
        {
            public int Index, Parent, Unk08, Unk0c;
            public float ScaleX, ScaleY, ScaleZ, ScaleW;
            public float RotationX, RotationY, RotationZ, RotationW;
            public float TranslationX, TranslationY, TranslationZ, TranslationW;
        }

        public class SubModel
        {
            public List<DmaVif> DmaVifs { get; set; }
            public List<Bone> Bones { get; set; }
        }

        public class DmaVif
        {
            public int TextureIndex { get; }
            public int[] Alaxi { get; }
            public byte[] VifPacket { get; }

            public DmaVif(int texi, int[] alaxi, byte[] bin)
            {
                TextureIndex = texi;
                Alaxi = alaxi;
                VifPacket = bin;
            }
        }

        public class Mdlxfst
        {
            private class SubModelHeader
            {
                [Data] public int Type { get; set; }
                [Data] public int Unk04 { get; set; }
                [Data] public int Unk08 { get; set; }
                [Data] public int NextOffset { get; set; }
                [Data] public short BoneCount { get; set; }
                [Data] public short Unk12 { get; set; }
                [Data] public int BoneOffset { get; set; }
                [Data] public int Off4 { get; set; }
                [Data] public short DmaChainCount { get; set; }
                [Data] public short Unk1e { get; set; }

                public SubModel SubModel { get; set; }
            }

            private class DmaChainHeader
            {
                [Data] public int Unk00 { get; set; }
                [Data] public int TextureIndex { get; set; }
                [Data] public int Unk08 { get; set; }
                [Data] public int Unk0c { get; set; }
                [Data] public int DmaOffset { get; set; }
                [Data] public int Unk14 { get; set; }
                [Data] public int DmaLength { get; set; }
                [Data] public int Unk1c { get; set; }
            }

            private const int ReservedArea = 0x90;
            public List<SubModel> SubModels;

            public Mdlxfst(Stream stream)
            {
                SubModels = new List<SubModel>();

                var currentOffset = ReservedArea;
                for (var i = 0; ; i++)
                {
                    var subStreamLength = stream.Length - currentOffset;
                    var header = ReadSubModel(new SubStream(stream, currentOffset, subStreamLength));

                    SubModels.Add(header.SubModel);

                    if (header.NextOffset == 0)
                        break;
                    currentOffset += header.NextOffset;
                }
            }

            private static SubModelHeader ReadSubModel(Stream stream)
            {
                var header = BinaryMapping.ReadObject<SubModelHeader>(stream);
                header.SubModel = new SubModel()
                {
                    DmaVifs = new List<DmaVif>(),
                    Bones = new List<Bone>(),
                };

                var dmaChains = For(header.DmaChainCount, () => BinaryMapping.ReadObject<DmaChainHeader>(stream)).ToArray();
                for (var j = 0; j < header.DmaChainCount; j++)
                {
                    header.SubModel.DmaVifs.AddRange(ReadDmaChain(stream, dmaChains[j]));
                }

                if (header.BoneOffset != 0)
                {
                    stream.Position = header.BoneOffset;
                    header.SubModel.Bones = For(header.BoneCount, () => ReadBone(stream)).ToList();
                }

                return header;
            }

            private static IEnumerable<DmaVif> ReadDmaChain(Stream stream, DmaChainHeader dmaChainHeader)
            {
                var dmaVifs = new List<DmaVif>();
                var count1a = stream.SetPosition(dmaChainHeader.Unk14).ReadInt32();
                var alv1 = For(count1a, () => stream.ReadInt32()).ToList();

                var offsetDmaTag = new List<int>();
                var alaxi = new List<int[]>();
                var alaxref = new List<int>();

                offsetDmaTag.Add(dmaChainHeader.DmaOffset);

                var offsetDmaBase = dmaChainHeader.DmaOffset + 0x10;

                for (var i = 0; i < count1a; i++)
                {
                    if (alv1[i] == -1)
                    {
                        offsetDmaTag.Add(offsetDmaBase + 0x10);
                        offsetDmaBase += 0x20;
                    }
                    else
                    {
                        offsetDmaBase += 0x10;
                    }

                    if (i + 1 == count1a)
                    {
                        alaxref.Add(alv1[i]);
                        alaxi.Add(alaxref.ToArray());
                        alaxref.Clear();
                    }
                    else if (alv1[i] == -1)
                    {
                        alaxi.Add(alaxref.ToArray());
                        alaxref.Clear();
                    }
                    else
                    {
                        alaxref.Add(alv1[i]);
                    }
                }

                for (var i = 0; i < offsetDmaTag.Count; i++)
                {
                    stream.Position = offsetDmaTag[i];
                    var count = stream.ReadInt16();
                    var unknown = stream.ReadInt16();
                    var offset = stream.ReadInt32();

                    stream.Position = offset & 0x7FFFFFFF;
                    var vifPacket = stream.ReadBytes(count * 0x10);
                    dmaVifs.Add(new DmaVif(dmaChainHeader.TextureIndex, alaxi[i], vifPacket));
                }

                return dmaVifs;
            }

            private static Bone ReadBone(Stream stream)
            {
                var reader = new BinaryReader(stream);
                return new Bone
                {
                    Index = reader.ReadInt32(),
                    Parent = reader.ReadInt32(),
                    Unk08 = reader.ReadInt32(),
                    Unk0c = reader.ReadInt32(),
                    ScaleX = reader.ReadSingle(),
                    ScaleY = reader.ReadSingle(),
                    ScaleZ = reader.ReadSingle(),
                    ScaleW = reader.ReadSingle(),
                    RotationX = reader.ReadSingle(),
                    RotationY = reader.ReadSingle(),
                    RotationZ = reader.ReadSingle(),
                    RotationW = reader.ReadSingle(),
                    TranslationX = reader.ReadSingle(),
                    TranslationY = reader.ReadSingle(),
                    TranslationZ = reader.ReadSingle(),
                    TranslationW = reader.ReadSingle()
                };
            }
        }
    }
}
