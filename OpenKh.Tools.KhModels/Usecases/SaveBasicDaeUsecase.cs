using COLLADASchema;
using ModelingToolkit.Objects;
using OpenKh.Tools.KhModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static OpenKh.Tools.KhModels.Utils.DaeModels;

namespace OpenKh.Tools.KhModels.Usecases
{
    public class SaveBasicDaeUsecase
    {
        public void Save(DaeModel model, Stream stream)
        {
            var collada = new COLLADA();

            collada.Asset = new Asset
            {
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Up_Axis = UpAxisType.Z_UP,
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

            var daeInstanceGeometryList = new List<DaeInstanceGeometry>();

            var daeTextureList = model.Materials;

            var geometryScaling = model.GeometryScaling;

            string ToMaterialReference(DaeMaterial? material) => (material != null)
                ? $"{ToSidForm(material.Name)}-material"
                : "";

            foreach (var mesh in model.Meshes)
            {
                var meshName = mesh.Name;
                var geometry = new Geometry
                {
                    Id = $"{ToSidForm(meshName)}-mesh",
                    Name = mesh.Name,
                };

                daeInstanceGeometryList.Add(
                    new DaeInstanceGeometry(
                        Url: $"#{ToSidForm(meshName)}-mesh",
                        Name: mesh.Name,
                        Instance_material: new string[] { ToMaterialReference(mesh.Material) }
                            .Where(it => it.Length != 0)
                    )
                );

                geometry.Mesh = new Mesh();

                {
                    // x y z
                    geometry.Mesh.Source.Add(
                        new Source
                        {
                            Id = $"{ToSidForm(meshName)}-mesh-positions",
                            Technique_Common = new SourceTechnique_Common
                            {
                                Accessor = new Accessor
                                {
                                    Source = $"#{ToSidForm(meshName)}-mesh-positions-array",
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
                                            Id = $"{ToSidForm(meshName)}-mesh-positions-array",
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
                        Id = $"{ToSidForm(meshName)}-mesh-vertices",
                    };

                    geometry.Mesh.Vertices.Input.Add(
                        new InputLocal
                        {
                            Semantic = "POSITION",
                            Source = $"#{ToSidForm(meshName)}-mesh-positions",
                        }
                    );
                }

                {
                    // s t
                    geometry.Mesh.Source.Add(
                        new Source
                        {
                            Id = $"{ToSidForm(meshName)}-mesh-map-0",
                            Technique_Common = new SourceTechnique_Common
                            {
                                Accessor = new Accessor
                                {
                                    Source = $"#{ToSidForm(meshName)}-mesh-map-0-array",
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
                                            Id = $"{ToSidForm(meshName)}-mesh-map-0-array",
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
                                                Source = $"#{ToSidForm(meshName)}-mesh-vertices",
                                                Offset = 0,
                                            }
                                        );
                                        triangle.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "TEXCOORD",
                                                Source = $"#{ToSidForm(meshName)}-mesh-map-0",
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
                                                Source = $"#{ToSidForm(meshName)}-mesh-vertices",
                                                Offset = 0,
                                            }
                                        );
                                        tristrip.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "TEXCOORD",
                                                Source = $"#{ToSidForm(meshName)}-mesh-map-0",
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
                var name = joint.Name ?? $"Bone.{index:000}";
                var boneNode = new Node { Id = $"Armature_{ToSidForm(name)}", Name = name, Sid = ToSidForm(name), Type = NodeType.JOINT, };

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
                            Id = $"Armature-{ToSidForm(mesh.Name)}-skin",
                            Name = "Armature",
                            Skin = new Skin
                            {
                                Source1 = $"#{ToSidForm(mesh.Name)}-mesh",
                            }
                                .Also(
                                    skin =>
                                    {
                                        skin.Bind_Shape_Matrix.Add("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1");

                                        skin.Source.Add(
                                            new Source
                                            {
                                                Id = $"Armature-{ToSidForm(mesh.Name)}-joints",
                                                Technique_Common = new SourceTechnique_Common
                                                {
                                                    Accessor = new Accessor
                                                    {
                                                        Source = $"Armature-{ToSidForm(mesh.Name)}-skin-joints-array",
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
                                                                Id = $"Armature-{ToSidForm(mesh.Name)}-skin-joints-array",
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
                                                Id = $"Armature-{ToSidForm(mesh.Name)}-skin-bind_poses",
                                                Technique_Common = new SourceTechnique_Common
                                                {
                                                    Accessor = new Accessor
                                                    {
                                                        Source = $"#Armature-{ToSidForm(mesh.Name)}-skin-bind_poses-array",
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
                                                Id = $"Armature-{ToSidForm(mesh.Name)}-skin-weights",
                                                Technique_Common = new SourceTechnique_Common
                                                {
                                                    Accessor = new Accessor
                                                    {
                                                        Source = $"#Armature-{ToSidForm(mesh.Name)}-skin-weights-array",
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
                                                Source = $"#Armature-{ToSidForm(mesh.Name)}-skin-joints",
                                            }
                                        );
                                        skin.Joints.Input.Add(
                                            new InputLocal
                                            {
                                                Semantic = "INV_BIND_MATRIX",
                                                Source = $"#Armature-{ToSidForm(mesh.Name)}-skin-bind_poses",
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
                                                Source = $"#Armature-{ToSidForm(mesh.Name)}-skin-joints",
                                                Offset = 0,
                                            }
                                        );
                                        skin.Vertex_Weights.Input.Add(
                                            new InputLocalOffset
                                            {
                                                Semantic = "WEIGHT",
                                                Source = $"#Armature-{ToSidForm(mesh.Name)}-skin-weights",
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

            foreach (var daeTexture in daeTextureList)
            {
                collada.Library_Images.Single().Image.Add(
                    new Image
                    {
                        Id = $"{ToSidForm(daeTexture.Name)}-png",
                        Name = $"{ToSidForm(daeTexture.Name)}-png",
                        Init_From = daeTexture.PngFilePath,
                    }
                );

                collada.Library_Effects.Single().Effect.Add(
                    new Effect
                    {
                        Id = $"{ToSidForm(daeTexture.Name)}-effect",
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
                                                        Sid = $"{ToSidForm(daeTexture.Name)}-surface",
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
                                                                            Value = $"{ToSidForm(daeTexture.Name)}-png",
                                                                        }
                                                                    );
                                                                }
                                                            ),
                                                    }
                                                );

                                                profile_COMMON.Newparam.Add(
                                                    new Common_Newparam_Type
                                                    {
                                                        Sid = $"{ToSidForm(daeTexture.Name)}-sampler",
                                                        Sampler2D = new Fx_Sampler2D_Common
                                                        {
                                                            Source = $"{ToSidForm(daeTexture.Name)}-surface",
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
                                                                Texture = $"{ToSidForm(daeTexture.Name)}-sampler",
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
                        Id = $"{ToSidForm(daeTexture.Name)}-material",
                        Name = daeTexture.Name,
                        Instance_Effect = new Instance_Effect
                        {
                            Url = $"#{ToSidForm(daeTexture.Name)}-effect",
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

                                            foreach (var daeInstanceGeometry in daeInstanceGeometryList)
                                            {
                                                visual_Scene.Node.Add(
                                                    new Node
                                                    {
                                                        Id = daeInstanceGeometry.Name,
                                                        Name = daeInstanceGeometry.Name,
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
                                                                        Url = daeInstanceGeometry.Url,
                                                                        Name = daeInstanceGeometry.Name,
                                                                    }
                                                                        .Also(
                                                                            instance_Geometry =>
                                                                            {
                                                                                if (daeInstanceGeometry.Instance_material.Any())
                                                                                {
                                                                                    instance_Geometry.Bind_Material = new Bind_Material();
                                                                                    foreach (var instanceMaterial in daeInstanceGeometry.Instance_material)
                                                                                    {
                                                                                        instance_Geometry.Bind_Material.Technique_Common.Add(
                                                                                            new Instance_Material
                                                                                            {
                                                                                                Symbol = instanceMaterial,
                                                                                                Target = $"#{instanceMaterial}",
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
