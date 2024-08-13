#if TOOLS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Godot.Nodes;
using OpenKh.Godot.Resources;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.TextureFooter;
using Array = Godot.Collections.Array;
using FileAccess = System.IO.FileAccess;

namespace OpenKh.Godot.addons.OpenKHImporter;

public partial class MdlxImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }
    public override string _GetImporterName() => "MdlxImporter";
    public override string _GetVisibleName() => "KH2 Model Importer";
    public override string[] _GetRecognizedExtensions() => ["mdlx"];
    public override string _GetSaveExtension() => "scn";
    public override string _GetResourceType() => "PackedScene";
    public override float _GetPriority() => 1;
    public override int _GetImportOrder() => 0;
    public override int _GetPresetCount() => Enum.GetValues<Presets>().Length;
    public override string _GetPresetName(int presetIndex) => ((Presets)presetIndex).ToString();
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => base._GetImportOptions(path, presetIndex);

    private static Shader BasicShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2BasicShader.gdshader");
    private static Shader AnimatedShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2AnimatedShader.gdshader");

    private static int SearchBytes(byte[] inside, byte[] find) 
    {
        var len = find.Length;
        var limit = inside.Length - len;
        for( var i = 0;  i <= limit;  i++ )
        {
            var k = 0;
            for(;  k < len;  k++ )
                if(find[k] != inside[i+k]) 
                    break;
            if (k == len) return i;
        }
        return -1;
    }
    
    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        var realPath = ProjectSettings.GlobalizePath(sourceFile);
        
        var fileName = Path.GetFileName(realPath);

        var name = Path.GetFileNameWithoutExtension(realPath);
        
        var root = new Node3D();
        root.Name = name;
        
        var relativePath = Path.GetRelativePath(Helpers.Kh2ImportOriginalPath, realPath);
        
        var hdTexturesPath = Path.Combine(Helpers.Kh2ImportRemasteredPath, relativePath);

        var usesHdTextures = false;
        var hdTextures = new System.Collections.Generic.Dictionary<int, string>();
        
        var hdTextureCount = 0;
        var hdTextureMap = new System.Collections.Generic.Dictionary<int, int>();

        //GD.Print(hdTexturesPath);
        
        if (Directory.Exists(hdTexturesPath))
        {
            usesHdTextures = true;
            foreach (var filePath in Directory.GetFiles(hdTexturesPath))
            {
                var textureName = Path.GetFileName(filePath);
                
                //GD.Print(textureName[1..^4]);
                
                if (!textureName.StartsWith('-') || !int.TryParse(textureName[1..^4], out var number)) continue;
                
                var localPath = ProjectSettings.LocalizePath(filePath);
                hdTextures[number] = localPath;
            }
        }
            

        using var stream = File.Open(realPath, FileMode.Open, FileAccess.Read);

        stream.Seek(0, SeekOrigin.Begin);
        
        if (!Bar.IsValid(stream))
            return Error.Failed;

        var barFile = Bar.Read(stream);

        var models = new List<ModelSkeletal>();
        var textures = new List<ModelTexture>();
        
        foreach (var barEntry in barFile)
        {
            try
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Model:
                    {
                        models.Add(ModelSkeletal.Read(barEntry.Stream));
                        break;
                    }
                    case Bar.EntryType.ModelTexture:
                    {
                        textures.Add(ModelTexture.Read(barEntry.Stream));
                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
        
        var images = new List<ImageTexture>();

        var texImg = textures.First();

        for (var index = 0; index < texImg.Images.Count; index++)
        {
            var texture = texImg.Images[index];
            var image = Image.CreateFromData(texture.Size.Width, texture.Size.Height, false, Image.Format.Rgba8, texture.ToBgra32().BGRAToRGBA());
            var tex = ImageTexture.CreateFromImage(image);
            images.Add(tex);
        }

        var hdMax = hdTextures.Max(i => i.Key) + 1;

        if (images.Count < hdMax) 
            while (images.Count < hdMax)
                images.Add(null);

        foreach (var index in hdTextures)
        {
            var texture = ResourceLoader.Load<ImageTexture>(index.Value);
            images[index.Key] = texture;
        }

        if (models.Count is 0) return Error.Failed;

        //TODO
        var model = models.First();
        
        var arrayMesh = new ArrayMesh();
        var skeleton = new Skeleton3D();
        
        root.AddChild(skeleton);

        foreach (var bone in model.Bones) skeleton.AddBone(bone.Index.ToString());

        var boneCount = skeleton.GetBoneCount();
        
        foreach (var bone in model.Bones.Where(i => i.ParentIndex < boneCount && i.ParentIndex >= 0)) skeleton.SetBoneParent(bone.Index, bone.ParentIndex);
        foreach (var bone in model.Bones) skeleton.SetBoneRest(bone.Index, bone.Transform());
        
        skeleton.ResetBonePoses();
        
        skeleton.Owner = root;
        skeleton.Name = "Skeleton";
        
        var textureAnimations = 0;

        var animList = new List<KH2TextureAnimation>();

        for (var m = 0; m < model.Groups.Count; m++)
        {
            var group = model.Groups[m];
            
            var header = group.Header;
            var mesh = group.Mesh;
            var material = new ShaderMaterial();

            var texIndex = (int)group.Header.TextureIndex;
            var tex = images[texIndex];
            if (usesHdTextures)
            {
                if (!hdTextureMap.TryGetValue(texIndex, out var hdTexIndex))
                {
                    hdTextureMap[texIndex] = hdTextureCount;
                    hdTexIndex = hdTextureCount;
                    hdTextureCount++;
                }
                tex = images[hdTexIndex];
            }
            var texSize = texImg.Images[texIndex].Size;
            var originalSize = new Vector2(texSize.Width, texSize.Height);

            if (texImg.TextureFooterData.TextureAnimationList.Any(i => i.TextureIndex == texIndex))
            {
                material.Shader = AnimatedShader;
                var find = texImg.TextureFooterData.TextureAnimationList.Where(i => i.TextureIndex == texIndex).ToArray();
                foreach (var animation in find)
                {
                    var uvFront = new Vector2(animation.UOffsetInBaseImage, animation.VOffsetInBaseImage) / originalSize;
                    var uvBack = new Vector2(animation.UOffsetInBaseImage + animation.SpriteWidth, animation.VOffsetInBaseImage + animation.SpriteHeight) / originalSize;

                    ImageTexture animatedTex;
                    
                    if (usesHdTextures)
                    {
                        var hdTexIndex = hdTextureCount;
                        hdTextureCount++;
                        animatedTex = images[hdTexIndex];
                    }
                    else
                    {
                        var data = ImageDataHelpers.FromIndexed8ToBitmap32(animation.SpriteImage, texImg.Images[texIndex].GetClut(), ImageDataHelpers.RGBA).BGRAToRGBA();
                    
                        var sprite = Image.CreateFromData(animation.SpriteWidth, animation.SpriteHeight * animation.NumSpritesInImageData, false, Image.Format.Rgba8, data);

                        animatedTex = ImageTexture.CreateFromImage(sprite);
                    }
                    
                    material.SetShaderParameter($"Sprite{textureAnimations}", animatedTex);
                    material.SetShaderParameter($"TextureOriginalUV{textureAnimations}", new Vector4(uvFront.X, uvFront.Y, uvBack.X, uvBack.Y));
                    
                    animList.Add(new KH2TextureAnimation
                    {
                        SpriteFrameCount = animation.NumSpritesInImageData,
                        TextureIndex = texIndex,
                        CurrentAnimation = animation.DefaultAnimationIndex,
                        AnimationList = new Array<KH2TextureAnimations>(animation.FrameGroupList.Select(i => new KH2TextureAnimations
                        {
                            Frames = new Array<KH2TextureAnimationFrame>(i.IndexedFrameList.Select(j => new KH2TextureAnimationFrame
                            {
                                ImageIndex = j.Value.SpriteImageIndex,
                                JumpDelta = j.Value.FrameIndexDelta,
                                MaxTime = j.Value.MaximumLength / 60f,
                                MinTime = j.Value.MinimumLength / 60f,
                                Operation = new Func<KH2TextureAnimationOperation>(() => j.Value.FrameControl switch
                                {
                                    TextureFrameControl.EnableSprite => KH2TextureAnimationOperation.EnableSprite,
                                    TextureFrameControl.DisableSprite => KH2TextureAnimationOperation.DisableSprite,
                                    TextureFrameControl.Jump => KH2TextureAnimationOperation.Jump,
                                    TextureFrameControl.Stop => KH2TextureAnimationOperation.Stop,
                                    _ => throw new ArgumentOutOfRangeException(),
                                }).Invoke(),
                                ResourceLocalToScene = true,
                            })),
                            ResourceLocalToScene = true,
                        })),
                        ResourceLocalToScene = true,
                    });

                    textureAnimations++;
                }
            }
            else
            {
                material.Shader = BasicShader;
            }
            
            //if ()
            
            //material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            
            //GD.Print($"{header.Alpha},{header.AlphaAdd},{header.AlphaSub},{header.AlphaEx}");
            
            //TODO:
            //I believe Alpha refers to Alpha Scissor, as this mode is used on Sora's crown and various strap textures
            //I presume AlphaAdd and Sub set the transparency, with Add and Sub modes
            //I presume AlphaEx refers to 'alpha extended', and is true alpha blending - ie 'mix'
            //Verify if these assumptions are correct, and what happens if they're stacked
            
            if (header.Alpha)
            {
                material.SetShaderParameter("Alpha", true);
                material.SetShaderParameter("Scissor", true);
                //material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
            }
            if (header.AlphaAdd)
            {
                material.SetShaderParameter("Alpha", true);
                /*
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                material.BlendMode = BaseMaterial3D.BlendModeEnum.Add;
                */
            }
            if (header.AlphaSub)
            {
                material.SetShaderParameter("Alpha", true);
                /*
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                material.BlendMode = BaseMaterial3D.BlendModeEnum.Sub;
                */
            }
            if (header.AlphaEx)
            {
                material.SetShaderParameter("Alpha", true);
                /*
                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                material.BlendMode = BaseMaterial3D.BlendModeEnum.Mix;
                */
            }

            material.SetShaderParameter("Texture", tex);
            
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();
            var bones = new List<int>();
            var weights = new List<float>();
            
            var array = new Array();
            array.Resize((int)Mesh.ArrayType.Max);

            foreach (var verts in mesh.Triangles)
            {
                foreach (var index in verts.ToArray().Reverse())
                {
                    var vert = mesh.Vertices[index];
                    var pos = vert.Position;

                    var normal = vert.Normal is not null ? new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z) : Vector3.Up;
                    var color = vert.Color is not null ? new Color(vert.Color.R, vert.Color.G, vert.Color.B, vert.Color.A) : Colors.White;

                    var uv = new Vector2(vert.U / 4096, vert.V / 4096);
                    
                    //GD.Print(uv);

                    //normal = rotate * normal;
                    positions.Add(new Vector3(pos.X, pos.Y, pos.Z) * Helpers.KH2PositionScale);
                    normals.Add(normal);
                    uvs.Add(uv);
                    colors.Add(color);

                    for (var i = 0; i < 4; i++)
                    {
                        if (vert.BPositions.Count > i)
                        {
                            var p = vert.BPositions[i];
                            bones.Add(p.BoneIndex);
                            weights.Add(p.Position.W == 0 ? 1 : p.Position.W);
                        }
                        else
                        {
                            bones.Add(0);
                            weights.Add(0);
                        }
                    }
                }
            }

            array[(int)Mesh.ArrayType.Vertex] = positions.ToArray();
            array[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            array[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            array[(int)Mesh.ArrayType.Bones] = bones.ToArray();
            array[(int)Mesh.ArrayType.Weights] = weights.ToArray();

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array, flags:
                Mesh.ArrayFormat.FormatVertex | Mesh.ArrayFormat.FormatBones | Mesh.ArrayFormat.FormatNormal | Mesh.ArrayFormat.FormatTexUV |
                Mesh.ArrayFormat.FormatWeights);
            arrayMesh.SurfaceSetMaterial(m, material);
        }

        {
            var mesh = new KH2MeshInstance3D();
            skeleton.AddChild(mesh);
            mesh.Name = "Model";
            mesh.Mesh = arrayMesh;
            mesh.Owner = root;
            skeleton.CreateSkinFromRestTransforms();

            foreach (var anim in animList) mesh.TextureAnimations.Add(anim);

            var packed = new PackedScene();
            packed.Pack(root);
            ResourceSaver.Save(packed, $"{savePath}.{_GetSaveExtension()}");

            return Error.Ok;
        }
    }
}

#endif
