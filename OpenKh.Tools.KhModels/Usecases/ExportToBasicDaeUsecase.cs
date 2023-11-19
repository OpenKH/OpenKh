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
    public class ExportToBasicDaeUsecase
    {
        public void Export(
            IEnumerable<MtModel> models,
            Func<string, string> modelNameToFilePrefix,
            float geometryScaling = 0.001f)
        {
            string FormatPosition(Vector3? xyz)
            {
                var value = xyz ?? Vector3.Zero;

                return $"{value.X} {value.Y} {value.Z}";
            }

            string FormatTexCoord(Vector3? st)
            {
                var value = st ?? Vector3.Zero;

                return $"{value.X} {value.Y}";
            }

            foreach (var model in models)
            {
                var filePrefix = modelNameToFilePrefix(model.Name);

                var daeBones = model.Joints
                    .Select(
                        one => new DaeBone(
                            one.Name ?? throw new NullReferenceException(),
                            one.ParentId ?? -1,
                            one.RelativeScale ?? new Vector3(1, 1, 1),
                            one.RelativeRotation ?? Vector3.Zero,
                            one.RelativeTranslation ?? Vector3.Zero
                        )
                    )
                    .ToArray();

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

                var daeTextureList = model.Materials
                    .Select(
                        one => new DaeTexture(
                            Name: one.Name ?? throw new NullReferenceException(),
                            PngFilePath: ExportTexture(one)
                        )
                    )
                    .ToArray();

                var collada = new COLLADA();

                collada.Asset = new Asset
                {
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    Up_Axis = UpAxisType.Y_UP,
                    Unit = new AssetUnit
                    {
                        Meter = 1,
                        Name = "meter"
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
                collada.Library_Geometries.Add(new Library_Geometries());

                Node? rootBone = null;

                string ToSidForm(string name) => name.Replace(".", "_");
                double ToAngle(float rad) => rad / Math.PI * 180;

                var daeInstanceGeometryList = new List<DaeInstanceGeometry>();

                string GetMaterialNameOfIndex(int? index)
                {
                    if (index.HasValue && index.Value < daeTextureList.Count())
                    {
                        return $"{ToSidForm(daeTextureList[index.Value].Name)}-material";
                    }
                    else
                    {
                        return "";
                    }
                }

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
                            Instance_material: new string[] { GetMaterialNameOfIndex(mesh.MaterialId) }
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
                                                        .Select(it => FormatPosition(it.AbsolutePosition))
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
                                                Count = Convert.ToUInt64(2 * mesh.Vertices.Count),
                                                Value = string.Join(
                                                    " ",
                                                    mesh.Vertices
                                                        .Select(it => FormatTexCoord(it.TextureCoordinates))
                                                ),
                                            }
                                        );
                                    }
                                )
                        );
                    }

                    {
                        // faces

                        geometry.Mesh.Triangles.Add(
                            new Triangles
                            {
                                Material = GetMaterialNameOfIndex(mesh.MaterialId),
                                Count = Convert.ToUInt64(mesh.Faces.Count),
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
                                                mesh.Faces
                                                    .Select(
                                                        face =>
                                                        {
                                                            if (face.VertexIndices.Count != 3)
                                                            {
                                                                throw new InvalidDataException();
                                                            }

                                                            return string.Join(
                                                                " ",
                                                                face.VertexIndices
                                                                    .Select(it => $"{it} {it}")
                                                            );
                                                        }
                                                    )
                                            )
                                        );
                                    }
                                )
                        );
                    }

                    geometry.Mesh.Vertices.Input.Add(
                        new InputLocal
                        {
                            Semantic = "POSITION",
                            Source = $"#{ToSidForm(meshName)}-mesh-positions",
                        }
                    );

                    foreach (var vertex in mesh.Vertices)
                    {
                        foreach (var weight in vertex.Weights)
                        {

                        }
                    }

                    foreach (var face in mesh.Faces)
                    {

                    }

                    foreach (var ts in mesh.TriangleStrips)
                    {

                    }

                    collada.Library_Geometries.Single().Geometry.Add(geometry);
                }

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

                using var stream = File.Create(filePrefix + ".dae");
                new XmlSerializer(typeof(COLLADA)).Serialize(stream, collada);
            }
        }

        private record DaeInstanceGeometry(string Url, string Name, IEnumerable<string> Instance_material);
    }
}
