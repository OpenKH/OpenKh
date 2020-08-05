// Inspired by Kddf2's khkh_xldM.
// Original source code: https://gitlab.com/kenjiuno/khkh_xldM/blob/master/khkh_xldMii/Mdlxfst.cs

using OpenKh.Common;
using OpenKh.Common.Ps2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public partial class Mdlx
    {
        public class SubModelMapHeader
        {
            [Data] public int Type { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int NextOffset { get; set; }
            [Data] public int DmaChainMapCount { get; set; }
            [Data] public short va4 { get; set; }
            [Data] public short CountVifPacketRenderingGroup { get; set; }
            [Data] public int OffsetVifPacketRenderingGroup { get; set; }
            [Data] public int OffsetToOffsetDmaChainIndexRemapTable { get; set; }
        }

        private class DmaChainMap
        {
            [Data] public int VifOffset { get; set; }
            [Data] public int TextureIndex { get; set; }
            [Data] public short Unk08 { get; set; }
            [Data] public short Unk0a { get; set; }
            [Data] public int Unk0c { get; set; }
        }

        public class M4
        {
            public int unk04;
            public int unk08;
            public int nextOffset;
            public short va4;
            public List<ushort> DmaChainIndexRemapTable;
            public List<ushort[]> vifPacketRenderingGroup;
            public List<VifPacketDescriptor> VifPackets;
        }

        public class VifPacketDescriptor
        {
            public byte[] VifPacket { get; set; }
            public int TextureId { get; set; }
            public short Unk08 { get; set; }
            public short IsTransparentFlag { get; set; }
            public int Unk0c { get; set; }
            public ushort[] DmaPerVif { get; set; }
        }

        private static M4 ReadAsMap(Stream stream)
        {
            var header = BinaryMapping.ReadObject<SubModelMapHeader>(stream);
            if (header.Type != Map) throw new NotSupportedException("Type must be 2 for maps");

            var dmaChainMaps = For(header.DmaChainMapCount, () => BinaryMapping.ReadObject<DmaChainMap>(stream));

            stream.Position = header.OffsetVifPacketRenderingGroup;

            // The original game engine ignores header.Count1 for some reason
            var count1 = (short)((stream.ReadInt32() - header.OffsetVifPacketRenderingGroup) / 4);
            stream.Position -= 4;

            var vifPacketRenderingGroup = For(count1, () => stream.ReadInt32())
                .Select(offset => ReadUInt16List(stream.SetPosition(offset)).ToArray())
                .ToList();

            stream.Position = header.OffsetToOffsetDmaChainIndexRemapTable;
            var offsetDmaChainIndexRemapTable = stream.ReadInt32();
            stream.Position = offsetDmaChainIndexRemapTable;
            var dmaChainIndexRemapTable = ReadUInt16List(stream)
                .ToList();

            var vifPackets = dmaChainMaps
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
                        Unk08 = dmaChain.Unk08,
                        IsTransparentFlag = dmaChain.Unk0a,
                        Unk0c = dmaChain.Unk0c,
                        DmaPerVif = sizePerDma.ToArray(),
                    };
                })
                .ToList();

            return new M4
            {
                unk04 = header.Unk04,
                unk08 = header.Unk08,
                nextOffset = header.NextOffset,
                va4 = header.va4,

                vifPacketRenderingGroup = vifPacketRenderingGroup,
                DmaChainIndexRemapTable = dmaChainIndexRemapTable,
                VifPackets = vifPackets
            };
        }

        private static void WriteAsMap(Stream stream, M4 mapModel)
        {
            var mapHeader = new SubModelMapHeader
            {
                Type = Map,
                DmaChainMapCount = mapModel.VifPackets.Count,

                Unk04 = mapModel.unk04,
                Unk08 = mapModel.unk08,
                NextOffset = mapModel.nextOffset,
                va4 = mapModel.va4,
                CountVifPacketRenderingGroup = Convert.ToInt16(mapModel.vifPacketRenderingGroup.Count),
            };

            BinaryMapping.WriteObject(stream, mapHeader);

            var dmaChainMapDescriptorOffset = (int)stream.Position;
            stream.Position += mapModel.VifPackets.Count * 0x10;

            mapHeader.OffsetVifPacketRenderingGroup = (int)stream.Position;
            stream.Position += mapModel.vifPacketRenderingGroup.Count * 4;
            var groupOffsets = new List<int>();
            foreach (var group in mapModel.vifPacketRenderingGroup)
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
            WriteUInt16List(stream, mapModel.DmaChainIndexRemapTable);

            stream.AlignPosition(0x10);

            var dmaChainVifOffsets = new List<int>();
            foreach (var dmaChainMap in mapModel.VifPackets)
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
            for (var i = 0; i < mapModel.VifPackets.Count; i++)
            {
                var dmaChainMap = mapModel.VifPackets[i];
                BinaryMapping.WriteObject(stream, new DmaChainMap
                {
                    VifOffset = dmaChainVifOffsets[i],
                    TextureIndex = dmaChainMap.TextureId,
                    Unk08 = dmaChainMap.Unk08,
                    Unk0a = dmaChainMap.IsTransparentFlag,
                    Unk0c = dmaChainMap.Unk0c
                });
            }

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, mapHeader);
        }

        private static IEnumerable<ushort> ReadUInt16List(Stream stream)
        {
            while (true)
            {
                var data = stream.ReadUInt16();
                if (data == 0xFFFF) break;
                yield return data;
            }
        }

        private static void WriteUInt16List(Stream stream, IEnumerable<ushort> alb2t2)
        {
            foreach (var data in alb2t2)
                stream.Write(data);
            stream.Write((ushort)0xFFFF);
        }
    }
}
