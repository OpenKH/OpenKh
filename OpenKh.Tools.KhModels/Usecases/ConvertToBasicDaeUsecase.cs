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

            return new DaeModel(
                GeometryScaling: geometryScaling,
                Bones: sourceModel.Joints
                    .Select(
                        one => new DaeBone(
                            Name: one.Name ?? throw new NullReferenceException(),
                            ParentIndex: one.ParentId ?? -1,
                            RelativeScale: one.RelativeScale ?? new Vector3(1, 1, 1),
                            RelativeRotation: one.RelativeRotation ?? Vector3.Zero,
                            RelativeTranslation: one.RelativeTranslation ?? Vector3.Zero
                        )
                    )
                    .ToImmutableArray(),
                Materials: materials,
                Meshes: sourceModel.Meshes
                    .Select(ConvertMesh)
                    .ToImmutableArray()
            );
        }
    }
}
