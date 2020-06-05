using OpenKh.Common;
using OpenKh.Engine.Maths;
using OpenKh.Engine.Parsers.Kddf2;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Engine.Parsers
{
    public class NewModelParser
    {
        public class MeshDescriptor
        {
            public int[] Indices;
            public int TextureIndex, SegmentIndex;
        }

        public NewModelParser(Mdlx mdlx)
        {
            Mdlx = mdlx;
            Vertices = new List<CustomVertex.PositionColoredTextured>();
            MeshDescriptors = new List<MeshDescriptor>();

            Parse(mdlx.MapModel.VifPackets);
        }

        public Mdlx Mdlx { get; }

        public List<CustomVertex.PositionColoredTextured> Vertices { get; }
        public List<MeshDescriptor> MeshDescriptors { get; }

        private void Parse(List<Mdlx.VifPacketDescriptor> packetDescriptors)
        {
            var textureIndex = 0;
            var indexBuffer = new int[4];
            var recenti = 0;

            var model = new Model();
            for (var pdIndex = 0; pdIndex < packetDescriptors.Count; pdIndex++)
            {
                var vifPacketDescriptor = packetDescriptors[pdIndex];
                var unpacker = new VifUnpacker(vifPacketDescriptor.VifPacket);

                VifUnpacker.State state;
                do
                {
                    var indices = new List<int>();

                    state = unpacker.Run();

                    var vpu = new MemoryStream(unpacker.Memory, false)
                        .Using(stream => VpuPacket.Read(stream));

                    var baseVertexIndex = Vertices.Count;
                    for (var i = 0; i < vpu.Indices.Length; i++)
                    {
                        var vertexIndex = vpu.Indices[i];

                        Vector3 position;
                        position.X = -vpu.Vertices[vertexIndex.Index].X;
                        position.Y = vpu.Vertices[vertexIndex.Index].Y;
                        position.Z = vpu.Vertices[vertexIndex.Index].Z;

                        int colorR, colorG, colorB, colorA;
                        if (vpu.Colors.Length != 0)
                        {
                            colorR = vpu.Colors[i].R;
                            colorG = vpu.Colors[i].G;
                            colorB = vpu.Colors[i].B;
                            colorA = vpu.Colors[i].A;
                        }
                        else
                        {
                            colorR = 0x80;
                            colorG = 0x80;
                            colorB = 0x80;
                            colorA = 0x80;
                        }

                        var color = Math.Min(byte.MaxValue, colorB * 2) |
                            (Math.Min(byte.MaxValue, colorG * 2) << 8) |
                            (Math.Min(byte.MaxValue, colorR * 2) << 16) |
                            (Math.Min(byte.MaxValue, colorA * 2) << 24);

                        Vertices.Add(new CustomVertex.PositionColoredTextured(
                            position, color, (short)(ushort)vertexIndex.U / 4096.0f, (short)(ushort)vertexIndex.V / 4096.0f));

                        indexBuffer[(recenti++) & 3] = baseVertexIndex + i;
                        switch (vertexIndex.Function)
                        {
                            case VpuPacket.VertexFunction.None:
                                break;
                            case VpuPacket.VertexFunction.Stock:
                                break;
                            case VpuPacket.VertexFunction.DrawTriangle:
                                indices.Add(indexBuffer[(recenti - 1) & 3]);
                                indices.Add(indexBuffer[(recenti - 2) & 3]);
                                indices.Add(indexBuffer[(recenti - 3) & 3]);
                                break;
                            case VpuPacket.VertexFunction.DrawTriangleInverse:
                                indices.Add(indexBuffer[(recenti - 1) & 3]);
                                indices.Add(indexBuffer[(recenti - 3) & 3]);
                                indices.Add(indexBuffer[(recenti - 2) & 3]);
                                break;
                        }
                    }

                    MeshDescriptors.Add(new MeshDescriptor
                    {
                        Indices = indices.ToArray(),
                        TextureIndex = textureIndex + vifPacketDescriptor.TextureId,
                        SegmentIndex = pdIndex
                    });

                } while (state == VifUnpacker.State.Microprogram);
            }
        }
    }
}
