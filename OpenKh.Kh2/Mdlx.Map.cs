using OpenKh.Common;
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
        private class Header
        {
            [Data] public int Type { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int NextOffset { get; set; }
            [Data] public int Count2 { get; set; }
            [Data] public short va4 { get; set; }
            [Data] public short Count1 { get; set; }
            [Data] public int Offset1 { get; set; }
            [Data] public int Offset2 { get; set; }
        }

        private class Header2
        {
            [Data] public int VifOffset { get; set; }
            [Data] public int TextureIndex { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0c { get; set; }
        }
        public class M4
        {
            public List<ushort> alb1t2;
            public List<ushort[]> alb2;
            public List<VifPacketDescriptor> VifPackets;
        }

        public class VifPacketDescriptor
        {
            public byte[] VifPacket { get; }
            public int TextureId { get; }

            public VifPacketDescriptor(byte[] vifpkt, int texi)
            {
                VifPacket = vifpkt;
                TextureId = texi;
            }
        }

        private static M4 ReadAsMap(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.Type != 2) throw new NotSupportedException("Type must be 2 for maps");

            var vifPackets = For(header.Count2, () => BinaryMapping.ReadObject<Header2>(stream))
                .Select(subModel =>
                {
                    var currentVifOffset = subModel.VifOffset;

                    var packet = new List<byte>();
                    int vp00;
                    do
                    {
                        stream.Position = currentVifOffset;
                        vp00 = stream.ReadInt32();
                        var unk04 = stream.ReadInt32();
                        var qwc = vp00 & 0xFFFF;
                        packet.AddRange(stream.ReadBytes(8 + 16 * qwc));

                        currentVifOffset += 16 + 16 * qwc;
                    } while ((vp00 >> 28) != 6);

                    return new VifPacketDescriptor(packet.ToArray(), subModel.TextureIndex);
                })
                .ToList();

            stream.Position = header.Offset1;
            var alb2 = For(header.Count1, () => stream.ReadInt32())
                .Select(offset => ReadAlb2t2(stream.SetPosition(offset)).ToArray())
                .ToList();

            stream.Position = header.Offset2;
            var offb1t2t2 = stream.ReadInt32();

            stream.Position = offb1t2t2;
            var alb1t2 = For(header.Count2, () => stream.ReadUInt16());

            return new M4
            {
                alb1t2 = alb1t2.ToList(),
                alb2 = alb2,
                VifPackets = vifPackets
            };
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
    }
}
