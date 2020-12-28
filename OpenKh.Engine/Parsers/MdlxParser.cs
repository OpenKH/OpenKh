using OpenKh.Common;
using OpenKh.Engine.Motion;
using OpenKh.Kh2;
using OpenKh.Ps2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenKh.Engine.Parsers
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PositionColoredTextured
    {
        public float X, Y, Z;
        public float Tu, Tv;
        public byte R, G, B, A;

        public PositionColoredTextured(Vector3 v, int clr, float tu, float tv)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            Tu = tu;
            Tv = tv;
            R = (byte)(clr >> 16);
            G = (byte)(clr >> 8);
            B = (byte)clr;
            A = (byte)(clr >> 24);
        }
    }

    public class MeshDescriptor
    {
        public PositionColoredTextured[] Vertices;
        public int[] Indices;
        public int TextureIndex;
        public bool IsOpaque;
    }

    public class MdlxParser : IModelMotion
    {
        private readonly Kkdf2MdlxParser _parsedModel;

        public MdlxParser(Mdlx mdlx)
        {
            if (IsEntity(mdlx))
            {
                InitialPose = BuildTPoseMatrices(mdlx.SubModels.First(), Matrix4x4.Identity);
                Bones = mdlx.SubModels.First().Bones;
                _parsedModel = new Kkdf2MdlxParser(mdlx.SubModels.First());
                MeshDescriptors = _parsedModel.ProcessVerticesAndBuildModel(InitialPose);
            }
            else if (IsMap(mdlx))
            {
                MeshDescriptors = mdlx.MapModel.VifPackets
                    .Select(vifPacket => Parse(vifPacket))
                    .ToList();
            }
        }

        public void ApplyMotion(Matrix4x4[] matrices) =>
            MeshDescriptors = _parsedModel.ProcessVerticesAndBuildModel(matrices);

        private static bool IsEntity(Mdlx mdlx) => mdlx.SubModels != null;

        private static bool IsMap(Mdlx mdlx) => mdlx.MapModel != null;

        public List<MeshDescriptor> MeshDescriptors { get; private set; }

        public List<Mdlx.Bone> Bones { get; private set; }

        public Matrix4x4[] InitialPose { get; set; }

        private static MeshDescriptor Parse(Mdlx.VifPacketDescriptor vifPacketDescriptor)
        {
            var vertices = new List<PositionColoredTextured>();
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

                    vertices.Add(new PositionColoredTextured(
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

        private static Matrix4x4[] BuildTPoseMatrices(Mdlx.SubModel model, Matrix4x4 initialMatrix)
        {
            var boneList = model.Bones.ToArray();
            var matrices = new Matrix4x4[boneList.Length];
            {
                var absTranslationList = new Vector3[matrices.Length];
                var absRotationList = new Quaternion[matrices.Length];
                for (int x = 0; x < matrices.Length; x++)
                {
                    Quaternion absRotation;
                    Vector3 absTranslation;
                    var oneBone = boneList[x];
                    var parent = oneBone.Parent;
                    if (parent < 0)
                    {
                        absRotation = Quaternion.Identity;
                        absTranslation = Vector3.Zero;
                    }
                    else
                    {
                        absRotation = absRotationList[parent];
                        absTranslation = absTranslationList[parent];
                    }

                    var localTranslation = Vector3.Transform(new Vector3(oneBone.TranslationX, oneBone.TranslationY, oneBone.TranslationZ), Matrix4x4.CreateFromQuaternion(absRotation));
                    absTranslationList[x] = absTranslation + localTranslation;

                    var localRotation = Quaternion.Identity;
                    if (oneBone.RotationZ != 0)
                        localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitZ, oneBone.RotationZ));
                    if (oneBone.RotationY != 0)
                        localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitY, oneBone.RotationY));
                    if (oneBone.RotationX != 0)
                        localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitX, oneBone.RotationX));
                    absRotationList[x] = absRotation * localRotation;
                }
                for (int x = 0; x < matrices.Length; x++)
                {
                    var absMatrix = initialMatrix;
                    absMatrix *= Matrix4x4.CreateFromQuaternion(absRotationList[x]);
                    absMatrix *= Matrix4x4.CreateTranslation(absTranslationList[x]);
                    matrices[x] = absMatrix;
                }
            }

            return matrices;
        }
    }
}
