using OpenKh.Common;
using OpenKh.Common.Ps2;
using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class ModelBackground : Model
    {
        public enum BackgroundType : int
        {
            Field,
            Skybox,
        }

        private record Header
        {
            [Data] public Type Type { get; set; }
            [Data] public BackgroundType BgType { get; set; }
            [Data] public int Attribute { get; set; }
            [Data] public int NextOffset { get; set; }
            [Data] public ushort ChunkCount { get; set; }
            [Data] public short Unk12 { get; set; }
            [Data] public short Unk14 { get; set; }
            [Data] public short CountVifPacketRenderingGroup { get; set; }
            [Data] public int OffsetVifPacketRenderingGroup { get; set; }
            [Data] public int OffsetToOffsetDmaChainIndexRemapTable { get; set; }
        }

        private record ModelChunkInfo
        {
            [Data] public int DmaTagOffset { get; set; }
            [Data] public short TextureIndex { get; set; }
            [Data] public ushort Flags1 { get; set; }
            [Data] public short Priority { get; set; }
            [Data] public short TransparencyFlag { get; set; }
            [Data] public byte Flags2 { get; set; }
            [Data] public byte Flags3 { get; set; }
            [Data] public short PolygonCount { get; set; }

            public bool IsSpecular
            {
                get => BitsUtil.Int.GetBit(Flags1, 0);
                set => Flags1 = (ushort)BitsUtil.Int.SetBit(Flags1, 0, value);
            }

            public bool HasVertexBuffer
            {
                get => BitsUtil.Int.GetBit(Flags1, 1);
                set => Flags1 = (ushort)BitsUtil.Int.SetBit(Flags1, 1, value);
            }

            public int Alternative
            {
                get => BitsUtil.Int.GetBits(Flags1, 2, 12);
                set => Flags1 = (ushort)BitsUtil.Int.SetBits(Flags1, 2, 12, value);
            }

            public bool IsAlphaSubtract
            {
                get => BitsUtil.Int.GetBit(Flags2, 7);
                set => Flags2 = BitsUtil.Int.SetBit(Flags2, 7, value);
            }

            public bool IsAlphaAdd
            {
                get => BitsUtil.Int.GetBit(Flags2, 6);
                set => Flags2 = BitsUtil.Int.SetBit(Flags2, 6, value);
            }

            public int UVScrollIndex
            {
                get => BitsUtil.Int.GetBits(Flags2, 1, 5);
                set => Flags2 = BitsUtil.Int.SetBits(Flags2, 1, 5, value);
            }

            public bool IsShadowOff
            {
                get => BitsUtil.Int.GetBit(Flags2, 0);
                set => Flags2 = BitsUtil.Int.SetBit(Flags2, 0, value);
            }

            public bool IsPhase
            {
                get => BitsUtil.Int.GetBit(Flags3, 7);
                set => Flags3 = BitsUtil.Int.SetBit(Flags3, 7, value);
            }

            public uint DrawPriority
            {
                get => (uint)BitsUtil.Int.GetBits(Flags3, 2, 5);
                set => Flags3 = BitsUtil.Int.SetBits(Flags3, 2, 5, (int)value);
            }

            public bool IsMulti
            {
                get => BitsUtil.Int.GetBit(Flags3, 1);
                set => Flags3 = BitsUtil.Int.SetBit(Flags3, 1, value);
            }

            public bool IsAlpha
            {
                get => BitsUtil.Int.GetBit(Flags3, 0);
                set => Flags3 = BitsUtil.Int.SetBit(Flags3, 0, value);
            }
        }

        public record ModelChunk
        {
            public byte[] VifPacket { get; set; }
            public short TextureId { get; set; }
            public short Priority { get; set; }
            public short TransparencyFlag { get; set; }

            public bool IsSpecular { get; set; }

            public bool HasVertexBuffer { get; set; }

            public int Alternative { get; set; }

            public bool IsAlphaSubtract { get; set; }

            public bool IsAlphaAdd { get; set; }

            public int UVScrollIndex { get; set; }

            public bool IsShadowOff { get; set; }

            public bool IsPhase { get; set; }

            public uint DrawPriority { get; set; }

            public bool IsMulti { get; set; }

            public bool IsAlpha { get; set; }
            public short PolygonCount { get; set; }
            public ushort[] DmaPerVif { get; set; }
        }

        private readonly List<ModelChunkInfo> _dmaChains;
        public List<ushort> DmaChainIndexRemapTable { get; set; }
        public List<ModelChunk> VifPackets { get; set; }
        public BackgroundType BgType { get; set; }
        public int Attribute { get; set; }
        private readonly int _nextOffset;
        public short Unk12 { get; set; }
        public short Unk14 { get; set; }
        private readonly short _countVifPacketRenderingGroup;
        public List<ushort[]> vifPacketRenderingGroup;

        public ModelBackground()
        {

        }

        public ModelBackground(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            _dmaChains = For(header.ChunkCount, () => BinaryMapping.ReadObject<ModelChunkInfo>(stream));

            foreach (var group in _dmaChains)
            {
                if (group.TransparencyFlag > 0)
                    Flags |= 1;
                if (group.IsSpecular)
                    Flags |= 2;
            }

            stream.Position = header.OffsetVifPacketRenderingGroup;

            // The original game engine ignores header.Count1 for some reason
            var count1 = (short)((stream.ReadInt32() - header.OffsetVifPacketRenderingGroup) / 4);
            stream.Position -= 4;

            vifPacketRenderingGroup = For(count1, () => stream.ReadInt32())
                .Select(offset => ReadUInt16List(stream.SetPosition(offset)).ToArray())
                .ToList();

            stream.Position = header.OffsetToOffsetDmaChainIndexRemapTable;
            var offsetDmaChainIndexRemapTable = stream.ReadInt32();
            stream.Position = offsetDmaChainIndexRemapTable;
            DmaChainIndexRemapTable = ReadUInt16List(stream)
                .ToList();

            VifPackets = _dmaChains
                .Select(dmaChain =>
                {
                    var currentVifOffset = dmaChain.DmaTagOffset;

                    DmaTag dmaTag;
                    var packet = new List<byte>();
                    var sizePerDma = new List<ushort>();
                    do
                    {
                        stream.Position = currentVifOffset;
                        dmaTag = BinaryMapping.ReadObject<DmaTag>(stream);
                        var packets = stream.ReadBytes(8 + 16 * dmaTag.Qwc);

                        packet.AddRange(packets);

                        sizePerDma.Add(dmaTag.Qwc);
                        currentVifOffset += 16 + 16 * dmaTag.Qwc;
                    } while (dmaTag.TagId < 2);

                    return new ModelChunk
                    {
                        VifPacket = packet.ToArray(),
                        TextureId = dmaChain.TextureIndex,
                        Priority = dmaChain.Priority,
                        TransparencyFlag = dmaChain.TransparencyFlag,
                        IsSpecular = dmaChain.IsSpecular,
                        HasVertexBuffer = dmaChain.HasVertexBuffer,
                        Alternative = dmaChain.Alternative,
                        IsAlphaSubtract = dmaChain.IsAlphaSubtract,
                        IsAlphaAdd = dmaChain.IsAlphaAdd,
                        UVScrollIndex = dmaChain.UVScrollIndex,
                        IsShadowOff = dmaChain.IsShadowOff,
                        IsPhase = dmaChain.IsPhase,
                        DrawPriority = dmaChain.DrawPriority,
                        IsMulti = dmaChain.IsMulti,
                        IsAlpha = dmaChain.IsAlpha,
                        PolygonCount = dmaChain.PolygonCount,
                        DmaPerVif = sizePerDma.ToArray(),
                    };
                })
                .ToList();

            BgType = header.BgType;
            Attribute = header.Attribute;
            _nextOffset = header.NextOffset;
            Unk12 = header.Unk12;
            Unk14 = header.Unk14;
            _countVifPacketRenderingGroup = header.CountVifPacketRenderingGroup;
        }

        public override int GroupCount => _dmaChains.Count;

        public override int GetDrawPolygonCount(IList<byte> displayFlags)
        {
            var groupCount = Math.Min(displayFlags.Count, _dmaChains.Count);
            var polygonCount = 0;
            for (var i = 0; i < groupCount; i++)
            {
                if (displayFlags[i] > 0)
                    polygonCount += _dmaChains[i].PolygonCount;
            }

            return polygonCount;
        }

        protected override void InternalWrite(Stream stream)
        {
            var mapHeader = new Header
            {
                Type = Type.Background,
                BgType = BgType,
                Attribute = Attribute,
                NextOffset = _nextOffset,
                ChunkCount = (ushort)_dmaChains.Count,
                Unk12 = Unk12,
                Unk14 = Unk14,
                CountVifPacketRenderingGroup = _countVifPacketRenderingGroup,
            };
            BinaryMapping.WriteObject(stream, mapHeader);

            var dmaChainMapDescriptorOffset = (int)stream.Position;
            stream.Position += VifPackets.Count * 0x10;

            mapHeader.OffsetVifPacketRenderingGroup = (int)stream.Position;
            stream.Position += vifPacketRenderingGroup.Count * 4;
            var groupOffsets = new List<int>();
            foreach (var group in vifPacketRenderingGroup)
            {
                groupOffsets.Add((int)stream.Position);
                WriteUInt16List(stream, group);
            }

            // capture remapTable offset here
            var remapTableOffsetToOffset = Helpers.Align((int)stream.Position, 4);

            // seek back and fill offsets
            stream.Position = mapHeader.OffsetVifPacketRenderingGroup;
            foreach (var offset in groupOffsets)
                stream.Write(offset);

            // write remapTable here
            stream.Position = remapTableOffsetToOffset;
            mapHeader.OffsetToOffsetDmaChainIndexRemapTable = remapTableOffsetToOffset;
            var remapTableOffset = remapTableOffsetToOffset + 4;
            stream.Write(remapTableOffset);
            WriteUInt16List(stream, DmaChainIndexRemapTable);

            stream.AlignPosition(0x10);

            var dmaChainVifOffsets = new List<int>();
            foreach (var dmaChainMap in VifPackets)
            {
                var vifPacketIndex = 0;
                dmaChainVifOffsets.Add((int)stream.Position);

                foreach (var packetCount in dmaChainMap.DmaPerVif)
                {
                    BinaryMapping.WriteObject(stream, new DmaTag
                    {
                        Qwc = packetCount,
                        Address = 0,
                        TagId = packetCount > 0 ? 1 : 6,
                        Irq = false,
                    });

                    var packetLength = packetCount * 0x10 + 8;
                    stream.Write(dmaChainMap.VifPacket, vifPacketIndex, packetLength);

                    vifPacketIndex += packetLength;
                }
            }

            stream.AlignPosition(0x80);
            stream.SetLength(stream.Position);

            stream.Position = dmaChainMapDescriptorOffset;
            for (var i = 0; i < VifPackets.Count; i++)
            {
                var dmaChain = VifPackets[i];
                BinaryMapping.WriteObject(stream, new ModelChunkInfo
                {
                    DmaTagOffset = dmaChainVifOffsets[i],
                    TextureIndex = dmaChain.TextureId,
                    Priority = dmaChain.Priority,
                    TransparencyFlag = dmaChain.TransparencyFlag,
                    IsSpecular = dmaChain.IsSpecular,
                    HasVertexBuffer = dmaChain.HasVertexBuffer,
                    Alternative = dmaChain.Alternative,
                    IsAlphaSubtract = dmaChain.IsAlphaSubtract,
                    IsAlphaAdd = dmaChain.IsAlphaAdd,
                    UVScrollIndex = dmaChain.UVScrollIndex,
                    IsShadowOff = dmaChain.IsShadowOff,
                    IsPhase = dmaChain.IsPhase,
                    DrawPriority = dmaChain.DrawPriority,
                    IsMulti = dmaChain.IsMulti,
                    IsAlpha = dmaChain.IsAlpha,
                    PolygonCount = dmaChain.PolygonCount,
                });
            }

            stream.FromBegin();
            BinaryMapping.WriteObject(stream, mapHeader);
            stream.SetPosition(stream.Length);
        }
    }
}
