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
                if (!string.IsNullOrEmpty(material.DiffuseTextureFileName))
                {
                    var pngFilePath = $"{filePrefix}_{material.DiffuseTextureFileName}.png";
                    material.ExportAsPng(pngFilePath);
                    return pngFilePath;
                }
                else if (material.DiffuseTextureBitmap != null)
                {
                    var pngFilePath = $"{filePrefix}_{material.Name}.png";
                    material.DiffuseTextureBitmap.Save(pngFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    return pngFilePath;
                }
                else
                {
                    return "noImage.png";
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

            var instanceGeometries = new List<DaeMesh>();
            var instanceControllers = new List<DaeInstanceController>();

            var skinControllers = new List<DaeSkinController>();

            DaeMesh ConvertMesh(MtMesh sourceMesh)
            {
                return new DaeMesh(
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
                );
            }

            var meshPairs = new List<(MtMesh SourceMesh, DaeMesh Mesh)>();

            var meshes = sourceModel.Meshes
                .Select(
                    sourceMesh =>
                    {
                        var mesh = ConvertMesh(sourceMesh);
                        meshPairs.Add((sourceMesh, mesh));
                        return mesh;
                    }
                )
                .ToImmutableArray();

            foreach (var meshPair in meshPairs)
            {
                var sourceMesh = meshPair.SourceMesh;
                var mesh = meshPair.Mesh;

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

                var enableSkinning = false;

                foreach (var sourceVertex in sourceMesh.Vertices)
                {
                    var vertexWeights = new List<DaeVertexWeight>();

                    if (sourceVertex.HasWeights)
                    {
                        enableSkinning |= true;

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

                if (enableSkinning)
                {
                    var controller = new DaeSkinController(
                        Mesh: mesh,
                        Bones: assocBones
                            .Select(boneIndex => bones[boneIndex])
                            .ToImmutableArray(),
                        InvBindMatrices: invertedMatrices
                            .ToImmutableArray(),
                        SkinWeights: skinWeights.ToImmutableArray(),
                        VertexWeightSets: vertexWeightSets.ToImmutableArray()
                    );

                    skinControllers.Add(controller);

                    instanceControllers.Add(
                        new DaeInstanceController(
                            Mesh: mesh,
                            Skeleton: bones.First()
                        )
                    );
                }
                else
                {
                    instanceGeometries.Add(mesh);
                }
            }

            return new DaeModel(
                GeometryScaling: geometryScaling,
                Bones: bones,
                Materials: materials,
                Meshes: meshes,
                SkinControllers: skinControllers
                    .ToImmutableArray(),
                InstanceGeometries: instanceGeometries
                    .ToImmutableArray(),
                InstanceControllers: instanceControllers
                    .ToImmutableArray()
            );
        }
    }
}
