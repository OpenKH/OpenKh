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
            [Data] public short Count1 { get; set; }
            [Data] public int Offset1 { get; set; }
            [Data] public int Offset2 { get; set; }
        }

        private class DmaChainMap
        {
            [Data] public int VifOffset { get; set; }
            [Data] public int TextureIndex { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0c { get; set; }
        }

        public class M4
        {
            public int unk04;
            public int unk08;
            public int nextOffset;
            public short va4;
            public short count1;
            public List<ushort> alb1t2;
            public List<ushort[]> alb2;
            public int[] unknownTable;
            public List<VifPacketDescriptor> VifPackets;
        }

        public class VifPacketDescriptor
        {
            public byte[] VifPacket { get; set; }
            public int TextureId { get; set; }
            public int Unk08 { get; set; }
            public int Unk0c { get; set; }
            public ushort[] DmaPerVif { get; set; }
        }

        private static M4 ReadAsMap(Stream stream)
        {
            var header = BinaryMapping.ReadObject<SubModelMapHeader>(stream);
            if (header.Type != Map) throw new NotSupportedException("Type must be 2 for maps");

            var dmaChainMaps = For(header.DmaChainMapCount, () => BinaryMapping.ReadObject<DmaChainMap>(stream));

            stream.Position = header.Offset1;

            // The original game engine ignores header.Count1 for some reason
            var count1 = (short)((stream.ReadInt32() - header.Offset1) / 4);
            stream.Position -= 4;

            var alb2 = For(count1, () => stream.ReadInt32())
                .Select(offset => ReadAlb2t2(stream.SetPosition(offset)).ToArray())
                .ToList();

            stream.Position = header.Offset2;
            var offb1t2t2 = stream.ReadInt32();
            var unknownTableCount = (offb1t2t2 - header.Offset2) / 4 - 1;
            var unknownTable = For(unknownTableCount, () => stream.ReadInt32())
                .ToArray();

            stream.Position = offb1t2t2;
            var alb1t2 = ReadAlb2t2(stream).ToList();

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
                count1 = header.Count1,

                alb1t2 = alb1t2.ToList(),
                alb2 = alb2,
                unknownTable = unknownTable,
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
                Count1 = mapModel.count1, // in a form, ignored by the game engine
            };

            BinaryMapping.WriteObject(stream, mapHeader);

            var dmaChainMapDescriptorOffset = (int)stream.Position;
            stream.Position += mapModel.VifPackets.Count * 0x10;

            mapHeader.Offset1 = (int)stream.Position;
            stream.Position += mapModel.alb2.Count * 4;
            var alb2Offsets = new List<int>();
            foreach (var alb2 in mapModel.alb2)
            {
                alb2Offsets.Add((int)stream.Position);
                WriteAlb2(stream, alb2);
            }

            var endAlb2Offset = Helpers.Align((int)stream.Position, 4);
            stream.Position = mapHeader.Offset1;
            foreach (var alb2Offset in alb2Offsets)
                stream.Write(alb2Offset);
            stream.Position = endAlb2Offset;

            mapHeader.Offset2 = endAlb2Offset;
            stream.Write(endAlb2Offset + 4 + mapModel.unknownTable.Length * 4);
            stream.Write(mapModel.unknownTable);
            WriteAlb2(stream, mapModel.alb1t2);

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
                    Unk0c = dmaChainMap.Unk0c
                });
            }

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, mapHeader);
        }

        private static IEnumerable<ushort> ReadAlb2t2(Stream stream)
        {
            while (true)
            {
                var data = stream.ReadUInt16();
                if (data == 0xFFFF) break;
                yield return data;
            }
        }

        private static void WriteAlb2(Stream stream, IEnumerable<ushort> alb2t2)
        {
            foreach (var data in alb2t2)
                stream.Write(data);
            stream.Write((ushort)0xFFFF);
        }
    }
}
