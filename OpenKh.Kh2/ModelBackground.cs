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
            [Data] public ushort ModelCount { get; set; }
            [Data] public ushort ShadowCount { get; set; }
            [Data] public ushort TextureCount { get; set; }
            [Data] public ushort OctalTreeCount { get; set; }
            [Data] public int OffsetVifPacketRenderingGroup { get; set; }
            [Data] public int OffsetToOffsetDmaChainIndexRemapTable { get; set; }
        }

        private record ChunkInfo
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

        public List<ushort> DmaChainIndexRemapTable { get; set; }
        public List<ModelChunk> Chunks { get; set; }
        public BackgroundType BgType { get; set; }
        public int Attribute { get; set; }
        private readonly int _nextOffset;
        public ushort ShadowCount { get; private set; }
        public ushort TextureCount { get; private set; }
        public ushort OctalTreeCount { get; private set; }
        public List<ushort[]> vifPacketRenderingGroup;

        public ModelBackground()
        {

        }

        public ModelBackground(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            var chunks = For(header.ModelCount, () => BinaryMapping.ReadObject<ChunkInfo>(stream));

            foreach (var chunk in chunks)
            {
                if (chunk.TransparencyFlag > 0)
                    Flags |= 1;
                if (chunk.IsSpecular)
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

            Chunks = chunks
                .Select(chunk =>
                {
                    var currentVifOffset = chunk.DmaTagOffset;

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
                        TextureId = chunk.TextureIndex,
                        Priority = chunk.Priority,
                        TransparencyFlag = chunk.TransparencyFlag,
                        IsSpecular = chunk.IsSpecular,
                        HasVertexBuffer = chunk.HasVertexBuffer,
                        Alternative = chunk.Alternative,
                        IsAlphaSubtract = chunk.IsAlphaSubtract,
                        IsAlphaAdd = chunk.IsAlphaAdd,
                        UVScrollIndex = chunk.UVScrollIndex,
                        IsShadowOff = chunk.IsShadowOff,
                        IsPhase = chunk.IsPhase,
                        DrawPriority = chunk.DrawPriority,
                        IsMulti = chunk.IsMulti,
                        IsAlpha = chunk.IsAlpha,
                        PolygonCount = chunk.PolygonCount,
                        DmaPerVif = sizePerDma.ToArray(),
                    };
                })
                .ToList();

            BgType = header.BgType;
            Attribute = header.Attribute;
            _nextOffset = header.NextOffset;
            ShadowCount = header.ShadowCount;
            TextureCount = header.TextureCount;
            OctalTreeCount = header.OctalTreeCount;
        }

        public override int GroupCount => Chunks.Count;

        public override int GetDrawPolygonCount(IList<byte> displayFlags)
        {
            var groupCount = Math.Min(displayFlags.Count, Chunks.Count);
            var polygonCount = 0;
            for (var i = 0; i < groupCount; i++)
            {
                if (displayFlags[i] > 0)
                    polygonCount += Chunks[i].PolygonCount;
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
                ModelCount = (ushort)Chunks.Count,
                ShadowCount = ShadowCount,
                TextureCount = TextureCount,
                OctalTreeCount = OctalTreeCount,
            };
            BinaryMapping.WriteObject(stream, mapHeader);

            var dmaChainMapDescriptorOffset = (int)stream.Position;
            stream.Position += Chunks.Count * 0x10;

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

            var dmaTagOffsets = new List<int>();
            foreach (var chunk in Chunks)
            {
                var vifPacketIndex = 0;
                dmaTagOffsets.Add((int)stream.Position);

                foreach (var packetCount in chunk.DmaPerVif)
                {
                    BinaryMapping.WriteObject(stream, new DmaTag
                    {
                        Qwc = packetCount,
                        Address = 0,
                        TagId = packetCount > 0 ? 1 : 6,
                        Irq = false,
                    });

                    var packetLength = packetCount * 0x10 + 8;
                    stream.Write(chunk.VifPacket, vifPacketIndex, packetLength);

                    vifPacketIndex += packetLength;
                }
            }

            stream.AlignPosition(0x80);
            stream.SetLength(stream.Position);

            stream.Position = dmaChainMapDescriptorOffset;
            for (var i = 0; i < Chunks.Count; i++)
            {
                var chunk = Chunks[i];
                BinaryMapping.WriteObject(stream, new ChunkInfo
                {
                    DmaTagOffset = dmaTagOffsets[i],
                    TextureIndex = chunk.TextureId,
                    Priority = chunk.Priority,
                    TransparencyFlag = chunk.TransparencyFlag,
                    IsSpecular = chunk.IsSpecular,
                    HasVertexBuffer = chunk.HasVertexBuffer,
                    Alternative = chunk.Alternative,
                    IsAlphaSubtract = chunk.IsAlphaSubtract,
                    IsAlphaAdd = chunk.IsAlphaAdd,
                    UVScrollIndex = chunk.UVScrollIndex,
                    IsShadowOff = chunk.IsShadowOff,
                    IsPhase = chunk.IsPhase,
                    DrawPriority = chunk.DrawPriority,
                    IsMulti = chunk.IsMulti,
                    IsAlpha = chunk.IsAlpha,
                    PolygonCount = chunk.PolygonCount,
                });
            }

            stream.FromBegin();
            BinaryMapping.WriteObject(stream, mapHeader);
            stream.SetPosition(stream.Length);
        }
    }
}
