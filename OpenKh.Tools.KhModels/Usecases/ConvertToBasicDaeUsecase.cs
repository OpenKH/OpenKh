using ModelingToolkit.Objects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Tools.KhModels.Utils.DaeModels;

namespace OpenKh.Tools.KhModels.Usecases
{
    public class ConvertToBasicDaeUsecase
    {
        public DaeModel Convert(MtModel sourceModel, string filePrefix, float geometryScaling = 1f)
        {
            string ExportTexture(MtMaterial material)
            {
                if (string.IsNullOrEmpty(material.DiffuseTextureFileName))
                {
                    return "";
                }
                else
                {
                    var pngFilePath = $"{filePrefix}_{material.DiffuseTextureFileName}.png";
                    material.ExportAsPng(pngFilePath);
                    return pngFilePath;
                }
            }

            var materials = sourceModel.Materials
                .Select(
                    one => new DaeMaterial(
                        Name: one.Name ?? throw new NullReferenceException(),
                        PngFilePath: ExportTexture(one)
                    )
                )
                .ToImmutableArray();

            DaeMaterial? FindMaterialOrNull(int? index)
            {
                if (index.HasValue && index.Value < materials.Count())
                {
                    return materials[index.Value];
                }
                else
                {
                    return null;
                }
            }

            Vector2 ExtractTextureCoordinate(MtVertex vertex)
            {
                if (vertex.TextureCoordinates is Vector3 vector3)
                {
                    return new Vector2(vector3.X, vector3.Y);
                }
                else
                {
                    return Vector2.Zero;
                }
            }

            Vector3 ExtractVertex(MtVertex vertex) => vertex.AbsolutePosition ?? Vector3.Zero;

            IReadOnlyList<DaeVertexPointer> ConvertFace(MtFace sourceFace)
            {
                if (sourceFace.VertexIndices.Count != 3)
                {
                    throw new InvalidDataException();
                }

                return sourceFace.VertexIndices
                    .Select(
                        sourceVertexIndex => new DaeVertexPointer(
                            VertexIndex: sourceVertexIndex,
                            TextureCoordinateIndex: sourceVertexIndex
                        )
                    )
                    .ToImmutableArray();
            }

            var bones = sourceModel.Joints
                .Select(
                    one => new DaeBone(
                        Name: one.Name ?? throw new NullReferenceException(),
                        ParentIndex: one.ParentId ?? -1,
                        RelativeScale: one.RelativeScale ?? new Vector3(1, 1, 1),
                        RelativeRotation: one.RelativeRotation ?? Vector3.Zero,
                        RelativeTranslation: one.RelativeTranslation ?? Vector3.Zero
                    )
                )
                .ToImmutableArray();

            var skinControllers = new List<DaeSkinController>();

            DaeMesh InterceptSkinMesh(MtMesh sourceMesh, DaeMesh mesh)
            {
                var assocBones = new List<int>();
                var invertedMatrices = new List<Matrix4x4>();
                var skinWeights = new List<float>();

                int AssignBoneIndexOf(int jointIndex)
                {
                    var hit = assocBones.IndexOf(jointIndex);
                    if (hit == -1)
                    {
                        hit = assocBones.Count;
                        assocBones.Add(jointIndex);

                        Matrix4x4.Invert(
                            sourceModel.Joints[jointIndex].AbsoluteTransformationMatrix ?? throw new NullReferenceException(),
                            out Matrix4x4 inverted
                        );
                        invertedMatrices.Add(inverted);
                    }
                    return hit;
                }

                int AssignWeightIndexOf(float weight)
                {
                    var hit = skinWeights.IndexOf(weight);
                    if (hit == -1)
                    {
                        hit = skinWeights.Count;
                        skinWeights.Add(weight);
                    }
                    return hit;
                }

                var vertexWeightSets = new List<IReadOnlyList<DaeVertexWeight>>();

                foreach (var sourceVertex in sourceMesh.Vertices)
                {
                    var vertexWeights = new List<DaeVertexWeight>();

                    if (sourceVertex.HasWeights)
                    {
                        foreach (var sourceWeight in sourceVertex.Weights)
                        {
                            vertexWeights.Add(
                                new DaeVertexWeight(
                                    AssignBoneIndexOf(sourceWeight.JointIndex ?? throw new NullReferenceException()),
                                    AssignWeightIndexOf(sourceWeight.Weight ?? throw new NullReferenceException())
                                )
                            );
                        }
                    }

                    vertexWeightSets.Add(vertexWeights.ToImmutableArray());
                }

                skinControllers.Add(
                    new DaeSkinController(
                        Mesh: mesh,
                        Bones: assocBones
                            .Select(boneIndex => bones[boneIndex])
                            .ToImmutableArray(),
                        InvBindMatrices: invertedMatrices
                            .ToImmutableArray(),
                        SkinWeights: skinWeights.ToImmutableArray(),
                        VertexWeightSets: vertexWeightSets.ToImmutableArray()
                    )
                );

                return mesh;
            }

            DaeMesh ConvertMesh(MtMesh sourceMesh)
            {
                return InterceptSkinMesh(
                    sourceMesh,
                    new DaeMesh(
                        Name: sourceMesh.Name,
                        Material: FindMaterialOrNull(sourceMesh.MaterialId),
                        Vertices: sourceMesh.Vertices
                            .Select(ExtractVertex)
                            .ToImmutableArray(),
                        TextureCoordinates: sourceMesh.Vertices
                            .Select(ExtractTextureCoordinate)
                            .ToImmutableArray(),
                        TriangleStripSets: new IReadOnlyList<DaeVertexPointer>[0],
                        TriangleSets: sourceMesh.Faces
                            .Select(ConvertFace)
                            .ToImmutableArray()
                    )
                );
            }

            return new DaeModel(
                GeometryScaling: geometryScaling,
                Bones: bones,
                Materials: materials,
                Meshes: sourceModel.Meshes
                    .Select(ConvertMesh)
                    .ToImmutableArray(),
                SkinControllers: skinControllers
                    .ToImmutableArray()
            );
        }
    }
}
