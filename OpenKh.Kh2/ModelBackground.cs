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
        private record Header
        {
            [Data] public Type Type { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int NextOffset { get; set; }
            [Data] public ushort DmaChainCount { get; set; }
            [Data] public short Unk12 { get; set; }
            [Data] public short Unk14 { get; set; }
            [Data] public short CountVifPacketRenderingGroup { get; set; }
            [Data] public int OffsetVifPacketRenderingGroup { get; set; }
            [Data] public int OffsetToOffsetDmaChainIndexRemapTable { get; set; }
        }

        private record DmaChainMap
        {
            [Data] public int VifOffset { get; set; }
            [Data] public short TextureIndex { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public short Unk08 { get; set; }
            [Data] public short TransparencyFlag { get; set; }
            [Data] public byte Unk0c { get; set; }
            [Data] public byte Unk0d { get; set; }
            [Data] public short PolygonCount { get; set; }

            public bool EnableUvsc
            {
                get => BitsUtil.Int.GetBit(Unk0c, 1);
                set => Unk0c = BitsUtil.Int.SetBit(Unk0c, 1, value);
            }

            public int UvscIndex
            {
                get => BitsUtil.Int.GetBits(Unk0c, 2, 4);
                set => Unk0c = BitsUtil.Int.SetBits(Unk0c, 2, 4, value);
            }
        }

        public record VifPacketDescriptor
        {
            public byte[] VifPacket { get; set; }
            public short TextureId { get; set; }
            public short Unk06 { get; set; }
            public short Unk08 { get; set; }
            public short TransparencyFlag { get; set; }
            public byte Unk0c { get; set; }
            public byte Unk0d { get; set; }
            public short PolygonCount { get; set; }
            public ushort[] DmaPerVif { get; set; }

            public bool EnableUvsc
            {
                get => BitsUtil.Int.GetBit(Unk0c, 1);
                set => Unk0c = BitsUtil.Int.SetBit(Unk0c, 1, value);
            }

            public int UvscIndex
            {
                get => BitsUtil.Int.GetBits(Unk0c, 2, 4);
                set => Unk0c = BitsUtil.Int.SetBits(Unk0c, 2, 4, value);
            }
        }

        private readonly List<DmaChainMap> _dmaChains;
        public List<ushort> DmaChainIndexRemapTable { get; set; }
        public List<VifPacketDescriptor> VifPackets { get; set; }
        public int Unk04 { get; set; }
        public int Unk08 { get; set; }
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
            _dmaChains = For(header.DmaChainCount, () => BinaryMapping.ReadObject<DmaChainMap>(stream));

            foreach (var group in _dmaChains)
            {
                if (group.TransparencyFlag > 0)
                    Flags |= 1;
                if ((group.Unk06 & 1) != 0)
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
                    var currentVifOffset = dmaChain.VifOffset;

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

                    return new VifPacketDescriptor
                    {
                        VifPacket = packet.ToArray(),
                        TextureId = dmaChain.TextureIndex,
                        Unk06 = dmaChain.Unk06,
                        Unk08 = dmaChain.Unk08,
                        TransparencyFlag = dmaChain.TransparencyFlag,
                        Unk0c = dmaChain.Unk0c,
                        Unk0d = dmaChain.Unk0d,
                        PolygonCount = dmaChain.PolygonCount,
                        DmaPerVif = sizePerDma.ToArray(),
                    };
                })
                .ToList();

            Unk04 = header.Unk04;
            Unk08 = header.Unk08;
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
                Unk04 = Unk04,
                Unk08 = Unk08,
                NextOffset = _nextOffset,
                DmaChainCount = (ushort)_dmaChains.Count,
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
                var dmaChainMap = VifPackets[i];
                BinaryMapping.WriteObject(stream, new DmaChainMap
                {
                    VifOffset = dmaChainVifOffsets[i],
                    TextureIndex = dmaChainMap.TextureId,
                    Unk06 = dmaChainMap.Unk06,
                    Unk08 = dmaChainMap.Unk08,
                    TransparencyFlag = dmaChainMap.TransparencyFlag,
                    Unk0c = dmaChainMap.Unk0c,
                    Unk0d = dmaChainMap.Unk0d,
                    PolygonCount = dmaChainMap.PolygonCount,
                    EnableUvsc = dmaChainMap.EnableUvsc,
                    UvscIndex = dmaChainMap.UvscIndex,
                });
            }

            stream.FromBegin();
            BinaryMapping.WriteObject(stream, mapHeader);
            stream.SetPosition(stream.Length);
        }
    }
}
