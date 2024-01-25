using COLLADASchema;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static OpenKh.ColladaUtils.DaeModels;

namespace OpenKh.ColladaUtils
{
    public class SaveDaeModelUsecase
    {
        public void Save(DaeModel model, Stream stream)
        {
            var collada = new COLLADA();

            collada.Asset = new Asset
            {
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Up_Axis = UpAxisType.Y_UP,
                Unit = new AssetUnit
                {
                    Meter = 1,
                    Name = "meter",
                },
            };
            collada.Asset.Contributor.Add(
                new AssetContributor
                {
                    Author = "OpenKh contributors",
                    Authoring_Tool = "OpenKh.Tools.KhModels",
                }
            );

            collada.Library_Images.Add(new Library_Images());
            collada.Library_Materials.Add(new Library_Materials());
            collada.Library_Effects.Add(new Library_Effects());
            collada.Library_Controllers.Add(new Library_Controllers());
            collada.Library_Geometries.Add(new Library_Geometries());

            Node? rootBone = null;

            string ToSidForm(string name) => name.Replace(".", "_");
            double ToAngle(float rad) => rad / Math.PI * 180;

            string ToMaterialId(DaeMaterial material) => $"{ToSidForm(material.Name)}-material";
            string ToMeshMeshId(DaeMesh mesh) => $"{ToSidForm(mesh.Name)}-mesh";
            string ToMeshMeshPositionsId(DaeMesh mesh) => $"{ToSidForm(mesh.Name)}-mesh-positions";
            string ToMeshMeshPositionsArrayId(DaeMesh mesh) => $"{ToSidForm(mesh.Name)}-mesh-positions-array";
            string ToMeshMeshVerticesId(DaeMesh mesh) => $"{ToSidForm(mesh.Name)}-mesh-vertices";
            string ToMeshMeshMap0Id(DaeMesh mesh) => $"{ToSidForm(mesh.Name)}-mesh-map-0";
            string ToMeshMeshMap0ArrayId(DaeMesh mesh) => $"{ToSidForm(mesh.Name)}-mesh-map-0-array";
            string ToArmatureBoneId(DaeBone bone) => $"Armature_{ToSidForm(bone.Name)}";
            string ToArmatureMeshSkinId(DaeMesh mesh) => $"Armature-{ToSidForm(mesh.Name)}-skin";
            string ToArmatureMeshSkinJointsArrayId(DaeMesh mesh) => $"Armature-{ToSidForm(mesh.Name)}-skin-joints-array";
            string ToArmatureMeshSkinBindPosesId(DaeMesh mesh) => $"Armature-{ToSidForm(mesh.Name)}-skin-bind_poses";
            string ToArmatureMeshSkinBindPosesArrayId(DaeMesh mesh) => $"Armature-{ToSidForm(mesh.Name)}-skin-bind_poses-array";
            string ToArmatureMeshSkinWeightsId(DaeMesh mesh) => $"Armature-{ToSidForm(mesh.Name)}-skin-weights";
            string ToArmatureMeshSkinWeightsArrayId(DaeMesh mesh) => $"Armature-{ToSidForm(mesh.Name)}-skin-weights-array";
            string ToArmatureMeshSkinJointsId(DaeMesh mesh) => $"Armature-{ToSidForm(mesh.Name)}-skin-joints";
            string ToMaterialPngId(DaeMaterial material) => $"{ToSidForm(material.Name)}-png";
            string ToMaterialEffectId(DaeMaterial material) => $"{ToSidForm(material.Name)}-effect";
            string ToMaterialSurfaceId(DaeMaterial material) => $"{ToSidForm(material.Name)}-surface";
            string ToMaterialSamplerId(DaeMaterial material) => $"{ToSidForm(material.Name)}-sampler";
            string ToMaterialMaterialId(DaeMaterial material) => $"{ToSidForm(material.Name)}-material";

            var geometryScaling = model.GeometryScaling;

            string ToMaterialReference(DaeMaterial? material) => (material != null)
                ? ToMaterialId(material)
                : "";

            foreach (var mesh in model.Meshes)
            {
                var geometry = new Geometry
                {
                    Id = ToMeshMeshId(mesh),
                    Name = mesh.Name,
                };

                geometry.Mesh = new Mesh();

                {
                    // x y z
                    geometry.Mesh.Source.Add(
                        new Source
                        {
                            Id = ToMeshMeshPositionsId(mesh),
                            Technique_Common = new SourceTechnique_Common
                            {
                                Accessor = new Accessor
                                {
                                    Source = $"#{ToMeshMeshPositionsArrayId(mesh)}",
                                    Count = Convert.ToUInt64(3 * mesh.Vertices.Count),
                                    Stride = 3,
                                }
                                    .Also(
                                        accessor =>
                                        {
                                            accessor.Param.Add(new Param { Name = "X", Type = "float", });
                                            accessor.Param.Add(new Param { Name = "Y", Type = "float", });
                                            accessor.Param.Add(new Param { Name = "Z", Type = "float", });
                                        }
                                    ),
                            },
                        }
                            .Also(
                                source =>
                                {
                                    source.Float_Array.Add(
                                        new Float_Array
                                        {
                                            Id = $"{ToMeshMeshPositionsArrayId(mesh)}",
                                            Count = Convert.ToUInt64(3 * mesh.Vertices.Count),
                                            Value = string.Join(
                                                " ",
                                                mesh.Vertices
                                                    .Select(it => $"{it.X} {it.Y} {it.Z}")
                                            ),
                                        }
                                    );
                                }
                            )
                    );

                    // x y z
                    geometry.Mesh.Vertices = new Vertices
                    {
                        Id = ToMeshMeshVerticesId(mesh),
                    };

                    geometry.Mesh.Vertices.Input.Add(
                        new InputLocal
                        {
                            Semantic = "POSITION",
                            Source = $"#{ToMeshMeshPositionsId(mesh)}",
                        }
                    );
                }

                {
                    // s t
                    geometry.Mesh.Source.Add(
                        new Source
                        {
                            Id = ToMeshMeshMap0Id(mesh),
                            Technique_Common = new SourceTechnique_Common
                            {
                                Accessor = new Accessor
                                {
                                    Source = $"#{ToMeshMeshMap0ArrayId(mesh)}",
                                    Count = Convert.ToUInt64(2 * mesh.Vertices.Count),
                                    Stride = 2,
                                }
                                    .Also(
                                        accessor =>
                                        {
                                            accessor.Param.Add(new Param { Name = "S", Type = "float", });
                                            accessor.Param.Add(new Param { Name = "T", Type = "float", });
                                        }
                                    ),
                            },
                        }
                            .Also(
                                source =>
                                {
                                    source.Float_Array.Add(
                                        new Float_Array
                                        {
                                            Id = ToMeshMeshMap0ArrayId(mesh),
                                            Count = Convert.ToUInt64(2 * mesh.TextureCoordinates.Count),
                                            Value = string.Join(
                                                " ",
                                                mesh.TextureCoordinates
                                                    .Select(it => $"{it.X} {it.Y}")
                                            ),
                                        }
                                    );
                                }
                            )
                    );
                }

                {
                    // faces

                    if (mesh.TriangleSets.Any())
                    {
                        geometry.Mesh.Triangles.Add(
                            new Triangles
                            {
                                Material = ToMaterialReference(mesh.Material),
                                Count = Convert.ToUInt64(mesh.TriangleSets.Count),
                            }
                                .Also(
                                    triangle =>
                                    {
                                        triangle.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "VERTEX",
                                                Source = $"#{ToMeshMeshVerticesId(mesh)}",
                                                Offset = 0,
                                            }
                                        );
                                        triangle.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "TEXCOORD",
                                                Source = $"#{ToMeshMeshMap0Id(mesh)}",
                                                Offset = 1,
                                                Set = 0,
                                            }
                                        );

                                        triangle.P.Add(
                                            string.Join(
                                                " ",
                                                mesh.TriangleSets
                                                    .Select(
                                                        triangleSet =>
                                                            string.Join(
                                                                " ",
                                                                triangleSet
                                                                    .Select(
                                                                        pointer => $"{pointer.VertexIndex} {pointer.TextureCoordinateIndex}"
                                                                    )
                                                            )
                                                    )
                                            )
                                        );
                                    }
                                )
                        );
                    }

                    if (mesh.TriangleStripSets.Any())
                    {
                        geometry.Mesh.Tristrips.Add(
                            new Tristrips
                            {
                                Material = ToMaterialReference(mesh.Material),
                                Count = Convert.ToUInt64(mesh.TriangleStripSets.Count),
                            }
                                .Also(
                                    tristrip =>
                                    {
                                        tristrip.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "VERTEX",
                                                Source = $"#{ToMeshMeshVerticesId(mesh)}",
                                                Offset = 0,
                                            }
                                        );
                                        tristrip.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "TEXCOORD",
                                                Source = $"#{ToMeshMeshMap0Id(mesh)}",
                                                Offset = 1,
                                                Set = 0,
                                            }
                                        );
                                        foreach (var triangleStripSet in mesh.TriangleStripSets)
                                        {
                                            tristrip.P.Add(
                                                string.Join(
                                                    " ",
                                                    triangleStripSet
                                                        .Select(
                                                            triangleStrip => $"{triangleStrip.VertexIndex} {triangleStrip.TextureCoordinateIndex}"
                                                        )
                                                )
                                            );
                                        }
                                    }
                                )
                        );
                    }
                }

                collada.Library_Geometries.Single().Geometry.Add(geometry);
            }

            var daeBones = model.Bones;

            Node[] boneNodes = new Node[daeBones.Count()];
            foreach (var (joint, index) in daeBones.Select((joint, index) => (joint, index)))
            {
                var boneNode = new Node
                {
                    Id = ToArmatureBoneId(joint),
                    Name = joint.Name,
                    Sid = ToSidForm(joint.Name),
                    Type = NodeType.JOINT,
                };

                var scale = joint.RelativeScale;
                var rotation = joint.RelativeRotation;
                var location = joint.RelativeTranslation;

                boneNode.Scale.Add(new TargetableFloat3 { Sid = "scale", Value = $"{scale.X} {scale.Y} {scale.Z}", });
                boneNode.Rotate.Add(new Rotate { Sid = "rotationZ", Value = $"0 0 1 {ToAngle(rotation.Z)}", });
                boneNode.Rotate.Add(new Rotate { Sid = "rotationY", Value = $"0 1 0 {ToAngle(rotation.Y)}", });
                boneNode.Rotate.Add(new Rotate { Sid = "rotationX", Value = $"1 0 0 {ToAngle(rotation.X)}", });
                boneNode.Translate.Add(new TargetableFloat3 { Sid = "location", Value = $"{location.X} {location.Y} {location.Z}", });

                boneNodes[index] = boneNode;

                if (joint.ParentIndex != -1)
                {
                    boneNodes[joint.ParentIndex].NodeProperty.Add(boneNode);
                }
                else
                {
                    rootBone = boneNode;
                }
            }

            if (model.SkinControllers.Any())
            {
                foreach (var skinController in model.SkinControllers)
                {
                    var mesh = skinController.Mesh;

                    collada.Library_Controllers.Single().Controller.Add(
                        new Controller
                        {
                            Id = ToArmatureMeshSkinId(mesh),
                            Name = "Armature",
                            Skin = new Skin
                            {
                                Source1 = $"#{ToMeshMeshId(mesh)}",
                            }
                                .Also(
                                    skin =>
                                    {
                                        skin.Bind_Shape_Matrix.Add("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1");

                                        skin.Source.Add(
                                            new Source
                                            {
                                                Id = ToArmatureMeshSkinJointsId(mesh),
                                                Technique_Common = new SourceTechnique_Common
                                                {
                                                    Accessor = new Accessor
                                                    {
                                                        Source = $"#{ToArmatureMeshSkinJointsArrayId(mesh)}",
                                                        Count = Convert.ToUInt64(skinController.Bones.Count()),
                                                        Stride = 1,
                                                    }
                                                        .Also(
                                                            accessor =>
                                                            {
                                                                accessor.Param.Add(new Param { Name = "JOINT", Type = "name", });
                                                            }
                                                        ),
                                                },
                                            }
                                                .Also(
                                                    source =>
                                                    {
                                                        source.Name_Array.Add(
                                                            new Name_Array
                                                            {
                                                                Id = ToArmatureMeshSkinJointsArrayId(mesh),
                                                                Count = Convert.ToUInt64(skinController.Bones.Count()),
                                                                Value = string.Join(
                                                                    " ",
                                                                    skinController.Bones
                                                                        .Select(bone => ToSidForm(bone.Name))
                                                                ),
                                                            }
                                                        );
                                                    }
                                                )
                                        );

                                        skin.Source.Add(
                                            new Source
                                            {
                                                Id = ToArmatureMeshSkinBindPosesId(mesh),
                                                Technique_Common = new SourceTechnique_Common
                                                {
                                                    Accessor = new Accessor
                                                    {
                                                        Source = $"#{ToArmatureMeshSkinBindPosesArrayId(mesh)}",
                                                        Count = Convert.ToUInt64(16 * skinController.InvBindMatrices.Count()),
                                                        Stride = 16,
                                                    }
                                                        .Also(
                                                            accessor =>
                                                            {
                                                                accessor.Param.Add(
                                                                    new Param
                                                                    {
                                                                        Name = "TRANSFORM",
                                                                        Type = "float4x4",
                                                                    }
                                                                );
                                                            }
                                                        ),
                                                },
                                            }
                                                .Also(
                                                    source =>
                                                    {
                                                        source.Float_Array.Add(
                                                            new Float_Array
                                                            {
                                                                Value = string.Join(" ", skinController.InvBindMatrices.Select(MatrixToText)),
                                                            }
                                                        );
                                                    }
                                                )
                                        );

                                        skin.Source.Add(
                                            new Source
                                            {
                                                Id = ToArmatureMeshSkinWeightsId(mesh),
                                                Technique_Common = new SourceTechnique_Common
                                                {
                                                    Accessor = new Accessor
                                                    {
                                                        Source = $"#{ToArmatureMeshSkinWeightsArrayId(mesh)}",
                                                        Count = Convert.ToUInt64(skinController.SkinWeights.Count()),
                                                        Stride = 1,
                                                    }
                                                        .Also(
                                                            accessor =>
                                                            {
                                                                accessor.Param.Add(new Param { Name = "WEIGHT", Type = "float", });
                                                            }
                                                        ),
                                                },
                                            }
                                                .Also(
                                                    source =>
                                                    {
                                                        source.Float_Array.Add(
                                                            new Float_Array
                                                            {
                                                                Value = string.Join(" ", skinController.SkinWeights.Select(it => it.ToString())),
                                                            }
                                                        );
                                                    }
                                                )
                                        );

                                        skin.Joints = new SkinJoints();
                                        skin.Joints.Input.Add(
                                            new InputLocal
                                            {
                                                Semantic = "JOINT",
                                                Source = $"#{ToArmatureMeshSkinJointsId(mesh)}",
                                            }
                                        );
                                        skin.Joints.Input.Add(
                                            new InputLocal
                                            {
                                                Semantic = "INV_BIND_MATRIX",
                                                Source = $"#{ToArmatureMeshSkinBindPosesId(mesh)}",
                                            }
                                        );

                                        skin.Vertex_Weights = new SkinVertex_Weights
                                        {
                                            Count = Convert.ToUInt64(skinController.VertexWeightSets.Count()),
                                        };
                                        skin.Vertex_Weights.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "JOINT",
                                                Source = $"#{ToArmatureMeshSkinJointsId(mesh)}",
                                                Offset = 0,
                                            }
                                        );
                                        skin.Vertex_Weights.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "WEIGHT",
                                                Source = $"#{ToArmatureMeshSkinWeightsId(mesh)}",
                                                Offset = 1,
                                            }
                                        );
                                        skin.Vertex_Weights.Vcount.Add(
                                            string.Join(
                                                " ",
                                                skinController.VertexWeightSets
                                                    .Select(vertexWeightSet => $"{vertexWeightSet.Count()}")
                                            )
                                        );
                                        skin.Vertex_Weights.V.Add(
                                            string.Join(
                                                " ",
                                                skinController.VertexWeightSets
                                                    .SelectMany(vertexWeightSet => vertexWeightSet)
                                                    .Select(vertexWeight => $"{vertexWeight.JointIndex} {vertexWeight.WeightIndex}")
                                            )
                                        );
                                    }
                                ),
                        }
                    );
                }
            }

            foreach (var material in model.Materials)
            {
                collada.Library_Images.Single().Image.Add(
                    new Image
                    {
                        Id = ToMaterialPngId(material),
                        Name = ToMaterialPngId(material),
                        Init_From = material.PngFilePath,
                    }
                );

                collada.Library_Effects.Single().Effect.Add(
                    new Effect
                    {
                        Id = ToMaterialEffectId(material),
                    }
                        .Also(
                            effect =>
                            {
                                effect.Fx_Profile_Abstract.Add(
                                    new Profile_COMMON()
                                        .Also(
                                            profile_COMMON =>
                                            {
                                                profile_COMMON.Newparam.Add(
                                                    new Common_Newparam_Type
                                                    {
                                                        Sid = ToMaterialSurfaceId(material),
                                                        Surface = new Fx_Surface_Common
                                                        {
                                                            Type = Fx_Surface_Type_Enum.Item2D,
                                                        }
                                                            .Also(
                                                                surface =>
                                                                {
                                                                    surface.Init_From.Add(
                                                                        new Fx_Surface_Init_From_Common
                                                                        {
                                                                            Value = ToMaterialPngId(material),
                                                                        }
                                                                    );
                                                                }
                                                            ),
                                                    }
                                                );

                                                profile_COMMON.Newparam.Add(
                                                    new Common_Newparam_Type
                                                    {
                                                        Sid = ToMaterialSamplerId(material),
                                                        Sampler2D = new Fx_Sampler2D_Common
                                                        {
                                                            Source = ToMaterialSurfaceId(material),
                                                        }
                                                    }
                                                );

                                                profile_COMMON.Technique = new Profile_COMMONTechnique
                                                {
                                                    Sid = "common",

                                                    Lambert = new Profile_COMMONTechniqueLambert
                                                    {
                                                        Diffuse = new Common_Color_Or_Texture_Type
                                                        {
                                                            Texture = new Common_Color_Or_Texture_TypeTexture
                                                            {
                                                                Texture = ToMaterialSamplerId(material),
                                                                Texcoord = "UVMap",
                                                            }
                                                        }
                                                    },
                                                };
                                            }
                                        )
                                );
                            }
                        )
                );

                collada.Library_Materials.Single().Material.Add(
                    new Material
                    {
                        Id = ToMaterialMaterialId(material),
                        Name = material.Name,
                        Instance_Effect = new Instance_Effect
                        {
                            Url = $"#{ToMaterialEffectId(material)}",
                        },
                    }
                );

            }

            collada.Library_Visual_Scenes.Add(
                new Library_Visual_Scenes()
                    .Also(
                        library_Visual_Scenes =>
                        {
                            library_Visual_Scenes.Visual_Scene.Add(
                                new Visual_Scene
                                {
                                    Id = "Scene",
                                    Name = "Scene",
                                }
                                    .Also(
                                        visual_Scene =>
                                        {
                                            visual_Scene.Node.Add(
                                                new Node
                                                {
                                                    Id = "Armature",
                                                    Name = "Armature",
                                                    Type = NodeType.NODE,
                                                }
                                                    .Also(
                                                        armatureNode =>
                                                        {
                                                            armatureNode.Scale.Add(new TargetableFloat3 { Sid = "scale", Value = "1 1 1", });
                                                            armatureNode.Rotate.Add(new Rotate { Sid = "rotationZ", Value = "0 0 1 0", });
                                                            armatureNode.Rotate.Add(new Rotate { Sid = "rotationY", Value = "0 1 0 0", });
                                                            armatureNode.Rotate.Add(new Rotate { Sid = "rotationX", Value = "1 0 0 0", });
                                                            armatureNode.Translate.Add(new TargetableFloat3 { Sid = "location", Value = "0 0 0", });

                                                            if (rootBone != null)
                                                            {
                                                                armatureNode.NodeProperty.Add(rootBone);
                                                            }
                                                        }
                                                    )
                                            );

                                            foreach (var mesh in model.InstanceGeometries)
                                            {
                                                visual_Scene.Node.Add(
                                                    new Node
                                                    {
                                                        Id = mesh.Name,
                                                        Name = mesh.Name,
                                                        Type = NodeType.NODE,
                                                    }
                                                        .Also(
                                                            geoNode =>
                                                            {
                                                                geoNode.Scale.Add(new TargetableFloat3 { Sid = "scale", Value = $"{geometryScaling} {geometryScaling} {geometryScaling}", });
                                                                geoNode.Rotate.Add(new Rotate { Sid = "rotationZ", Value = "0 0 1 0", });
                                                                geoNode.Rotate.Add(new Rotate { Sid = "rotationY", Value = "0 1 0 0", });
                                                                geoNode.Rotate.Add(new Rotate { Sid = "rotationX", Value = "1 0 0 0", });
                                                                geoNode.Translate.Add(new TargetableFloat3 { Sid = "location", Value = "0 0 0", });
                                                                geoNode.Instance_Geometry.Add(
                                                                    new Instance_Geometry
                                                                    {
                                                                        Url = $"#{ToMeshMeshId(mesh)}",
                                                                        Name = mesh.Name,
                                                                    }
                                                                        .Also(
                                                                            instance_Geometry =>
                                                                            {
                                                                                if (mesh.Material != null)
                                                                                {
                                                                                    instance_Geometry.Bind_Material = new Bind_Material();
                                                                                    instance_Geometry.Bind_Material.Technique_Common.Add(
                                                                                        new Instance_Material
                                                                                        {
                                                                                            Symbol = ToMaterialReference(mesh.Material),
                                                                                            Target = $"#{ToMaterialReference(mesh.Material)}",
                                                                                        }
                                                                                            .Also(
                                                                                                instance_Material =>
                                                                                                {
                                                                                                    instance_Material.Bind_Vertex_Input.Add(
                                                                                                        new Instance_MaterialBind_Vertex_Input
                                                                                                        {
                                                                                                            Semantic = "UVMap",
                                                                                                            Input_Semantic = "TEXCOORD",
                                                                                                            Input_Set = 0,
                                                                                                        }
                                                                                                    );
                                                                                                }
                                                                                            )
                                                                                    );
                                                                                }
                                                                            }
                                                                        )
                                                                );
                                                            }
                                                        )
                                                );
                                            }

                                            foreach (var instanceControllers in model.InstanceControllers)
                                            {
                                                var mesh = instanceControllers.Mesh;

                                                visual_Scene.Node.Add(
                                                    new Node
                                                    {
                                                        Id = mesh.Name,
                                                        Name = mesh.Name,
                                                        Type = NodeType.NODE,
                                                    }
                                                        .Also(
                                                            geoNode =>
                                                            {
                                                                geoNode.Scale.Add(new TargetableFloat3 { Sid = "scale", Value = $"{geometryScaling} {geometryScaling} {geometryScaling}", });
                                                                geoNode.Rotate.Add(new Rotate { Sid = "rotationZ", Value = "0 0 1 0", });
                                                                geoNode.Rotate.Add(new Rotate { Sid = "rotationY", Value = "0 1 0 0", });
                                                                geoNode.Rotate.Add(new Rotate { Sid = "rotationX", Value = "1 0 0 0", });
                                                                geoNode.Translate.Add(new TargetableFloat3 { Sid = "location", Value = "0 0 0", });
                                                                geoNode.Instance_Controller.Add(
                                                                    new Instance_Controller
                                                                    {
                                                                        Url = $"#{ToArmatureMeshSkinId(mesh)}",
                                                                    }
                                                                        .Also(
                                                                            instance_Controller =>
                                                                            {
                                                                                instance_Controller.Skeleton.Add(
                                                                                    $"#{ToArmatureBoneId(instanceControllers.Skeleton)}"
                                                                                );

                                                                                if (mesh.Material != null)
                                                                                {
                                                                                    instance_Controller.Bind_Material = new Bind_Material();
                                                                                    instance_Controller.Bind_Material.Technique_Common.Add(
                                                                                        new Instance_Material
                                                                                        {
                                                                                            Symbol = ToMaterialReference(mesh.Material),
                                                                                            Target = $"#{ToMaterialReference(mesh.Material)}",
                                                                                        }
                                                                                            .Also(
                                                                                                instance_Material =>
                                                                                                {
                                                                                                    instance_Material.Bind_Vertex_Input.Add(
                                                                                                        new Instance_MaterialBind_Vertex_Input
                                                                                                        {
                                                                                                            Semantic = "UVMap",
                                                                                                            Input_Semantic = "TEXCOORD",
                                                                                                            Input_Set = 0,
                                                                                                        }
                                                                                                    );
                                                                                                }
                                                                                            )
                                                                                    );
                                                                                }
                                                                            }
                                                                        )
                                                                );
                                                            }
                                                        )
                                                );
                                            }
                                        }
                                    )
                            );
                        }
                    )
            );

            collada.Scene = new COLLADAScene
            {
                Instance_Visual_Scene = new InstanceWithExtra
                {
                    Url = "#Scene",
                },
            };

            new XmlSerializer(typeof(COLLADA)).Serialize(stream, collada);
        }

        private string MatrixToText(Matrix4x4 m)
        {
            return $"{m.M11} {m.M12} {m.M13} {m.M14} {m.M21} {m.M22} {m.M23} {m.M24} {m.M31} {m.M32} {m.M33} {m.M34} {m.M41} {m.M42} {m.M43} {m.M44}";
        }

        private record DaeInstanceGeometry(string Url, string Name, IEnumerable<string> Instance_material);
    }
}
