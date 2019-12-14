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

        private static IEnumerable<SubModel> ReadAsModel(Stream stream)
        {
            var currentOffset = 0;
            var nextOffset = ReservedArea;
            while (nextOffset != 0)
            {
                currentOffset += nextOffset;
                var subStream = new SubStream(stream, currentOffset, stream.Length - currentOffset);
                nextOffset = ReadSubModel(subStream, out var subModel);

                yield return subModel;
            }
        }

        private static int ReadSubModel(Stream stream, out SubModel subModel)
        {
            var header = BinaryMapping.ReadObject<SubModelHeader>(stream);
            header.SubModel = new SubModel()
            {
                DmaVifs = new List<DmaVif>(),
                Bones = new List<Bone>(),
            };

            var dmaChains = For(header.DmaChainCount, () => BinaryMapping.ReadObject<DmaChainHeader>(stream));
            for (var j = 0; j < header.DmaChainCount; j++)
            {
                header.SubModel.DmaVifs.AddRange(ReadDmaChain(stream, dmaChains[j]));
            }

            if (header.BoneOffset != 0)
            {
                stream.Position = header.BoneOffset;
                header.SubModel.Bones = For(header.BoneCount, () => ReadBone(stream)).ToList();
            }

            subModel = header.SubModel;

            return header.NextOffset;
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

        private static Bone ReadBone(Stream stream) => BinaryMapping.ReadObject<Bone>(stream);
    }
}
