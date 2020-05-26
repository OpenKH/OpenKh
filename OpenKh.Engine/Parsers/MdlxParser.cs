using OpenKh.Common;
using OpenKh.Engine.Maths;
using OpenKh.Engine.Parsers.Kddf2;
using OpenKh.Engine.Parsers.Kddf2.Mset;
using OpenKh.Engine.Parsers.Kddf2.Mset.EmuRunner;
using OpenKh.Kh2;
using OpenKh.Ps2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenKh.Engine.Parsers
{
    public class MeshDescriptor
    {
        public CustomVertex.PositionColoredTextured[] Vertices;
        public int[] Indices;
        public int TextureIndex;
        public bool IsOpaque;
    }

    public class MdlxParser
    {
        public MdlxParser(Mdlx mdlx)
        {
            if (IsEntity(mdlx))
            {
                var builtModel = FromEntity(mdlx);
                Model = new Model
                {
                    Segments = builtModel.textureIndexBasedModelDict.Values.Select(x => new Model.Segment
                    {
                        Vertices = x.Vertices.Select(vertex => new PositionColoredTextured
                        {
                            X = vertex.X,
                            Y = vertex.Y,
                            Z = vertex.Z,
                            U = vertex.Tu,
                            V = vertex.Tv,
                            Color = vertex.Color
                        }).ToArray()
                    }).ToArray(),
                    Parts = builtModel.parser.MeshDescriptors.Select(x => new Model.Part
                    {
                        Indices = x.Indices,
                        SegmentIndex = x.SegmentIndex,
                        TextureIndex = x.TextureIndex,
                        IsOpaque = x.IsOpaque
                    }).ToArray()
                };
            }
            else if (IsMap(mdlx))
            {
                MeshDescriptors = mdlx.MapModel.VifPackets
                    .Select(vifPacket => Parse(vifPacket))
                    .ToList();
            }
        }

        private static Kkdf2MdlxBuiltModel FromEntity(Mdlx mdlx)
        {
            Matrix[] matrixOut;
            {
                var mdlxFile = @"H:\KH2fm.yaz0r\obj\P_EX100.mdlx";
                var msetFile = @"H:\KH2fm.yaz0r\obj\P_EX100.mset";

                var mdlxStream = new MemoryStream(File.ReadAllBytes(mdlxFile), false);
                var msetStream = new MemoryStream(File.ReadAllBytes(msetFile), false);

                var msetBar = Bar.Read(msetStream);
                var anbBar = msetBar
                    .First(it => it.Type == Bar.EntryType.Bar);
                var animEntry = Bar.Read(anbBar.Stream)
                    .First(it => it.Type == Bar.EntryType.AnimationData);
                var animStream = animEntry.Stream;

                var anbAbsOff = (uint)(anbBar.Offset + animEntry.Offset);

                mdlxStream.Position = 0;
                msetStream.Position = 0;

                var anbParser = new AnimReader(animStream);
                var matrixOutStream = new MemoryStream();
                var emuRunner = new Mlink();
                emuRunner.Permit(
                    mdlxStream, anbParser.cntb1,
                    msetStream, anbParser.cntb2,
                    anbAbsOff, 0, matrixOutStream
                );

                BinaryReader br = new BinaryReader(matrixOutStream);
                matrixOutStream.Position = 0;
                matrixOut = new Matrix[anbParser.cntb1];
                for (int t = 0; t < anbParser.cntb1; t++)
                {
                    Matrix M1 = new Matrix();
                    M1.M11 = br.ReadSingle(); M1.M12 = br.ReadSingle(); M1.M13 = br.ReadSingle(); M1.M14 = br.ReadSingle();
                    M1.M21 = br.ReadSingle(); M1.M22 = br.ReadSingle(); M1.M23 = br.ReadSingle(); M1.M24 = br.ReadSingle();
                    M1.M31 = br.ReadSingle(); M1.M32 = br.ReadSingle(); M1.M33 = br.ReadSingle(); M1.M34 = br.ReadSingle();
                    M1.M41 = br.ReadSingle(); M1.M42 = br.ReadSingle(); M1.M43 = br.ReadSingle(); M1.M44 = br.ReadSingle();
                    matrixOut[t] = M1;
                }
            }

            var parser = new Kddf2.Kkdf2MdlxParser(mdlx.SubModels.First());
            var builtModel = parser
                .ProcessVerticesAndBuildModel(
                    matrixOut
                //MdlxMatrixUtil.BuildTPoseMatrices(mdlx.SubModels.First(), Matrix.Identity)
                );

            var ci = builtModel.textureIndexBasedModelDict.Select((kv, i) => new Kddf2.Kkdf2MdlxParser.CI
            {
                Indices = kv.Value.Vertices.Select((_, index) => index).ToArray(),
                TextureIndex = kv.Key.Item1,
                IsOpaque = kv.Key.Item2,
                SegmentIndex = i
            });

            parser.MeshDescriptors.AddRange(ci);

            return builtModel;
        }

        private static bool IsEntity(Mdlx mdlx) => mdlx.SubModels != null;

        private static bool IsMap(Mdlx mdlx) => mdlx.MapModel != null;

        public Model Model { get; }

        public List<MeshDescriptor> MeshDescriptors { get; }

        private static MeshDescriptor Parse(Mdlx.VifPacketDescriptor vifPacketDescriptor)
        {
            var vertices = new List<CustomVertex.PositionColoredTextured>();
            var indices = new List<int>();
            var unpacker = new VifUnpacker(vifPacketDescriptor.VifPacket);

            var indexBuffer = new int[4];
            var recentIndex = 0;
            while (unpacker.Run() != VifUnpacker.State.End)
            {
                var vpu = new MemoryStream(unpacker.Memory, false)
                    .Using(stream => VpuPacket.Read(stream));

                var baseVertexIndex = vertices.Count;
                for (var i = 0; i < vpu.Indices.Length; i++)
                {
                    var vertexIndex = vpu.Indices[i];
                    var position = new Vector3(
                        vpu.Vertices[vertexIndex.Index].X,
                        vpu.Vertices[vertexIndex.Index].Y,
                        vpu.Vertices[vertexIndex.Index].Z);

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

                    vertices.Add(new CustomVertex.PositionColoredTextured(
                        position, color, (short)(ushort)vertexIndex.U / 4096.0f, (short)(ushort)vertexIndex.V / 4096.0f));

                    indexBuffer[(recentIndex++) & 3] = baseVertexIndex + i;
                    switch (vertexIndex.Function)
                    {
                        case VpuPacket.VertexFunction.DrawTriangleDoubleSided:
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            break;
                        case VpuPacket.VertexFunction.Stock:
                            break;
                        case VpuPacket.VertexFunction.DrawTriangle:
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            break;
                        case VpuPacket.VertexFunction.DrawTriangleInverse:
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            break;
                    }
                }
            }

            return new MeshDescriptor
            {
                Vertices = vertices.ToArray(),
                Indices = indices.ToArray(),
                TextureIndex = vifPacketDescriptor.TextureId,
                IsOpaque = vifPacketDescriptor.IsTransparentFlag == 0,
            };
        }
    }
}
