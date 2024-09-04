using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FFMpegCore;
using FFMpegCore.Pipes;
using Godot;
using Godot.Collections;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Nodes;
using OpenKh.Godot.Resources;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Ps2;
using Array = Godot.Collections.Array;

namespace OpenKh.Godot.Conversion;

public static class Converters
{
    private static Shader BasicShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2BasicShader.gdshader");
    private static Shader AnimatedShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2AnimatedShader.gdshader");

    public static KH2Mdlx FromMdlx(Bar bar, List<ImageTexture> hdTexs = null)
    {
        var usesHdTextures = hdTexs is not null;

        var hdTextureCount = 0;
        var hdTextureMap = new System.Collections.Generic.Dictionary<int, int>();

        var root = new KH2Mdlx();

        var barFile = bar;

        var skeletalModels = new List<ModelSkeletal>();
        var textures = new List<ModelTexture>();
        var collisions = new List<byte[]>();

        foreach (var barEntry in barFile)
        {
            try
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Model:
                    {
                        barEntry.Stream.Seek(0x90, SeekOrigin.Begin);
                        var modelType = barEntry.Stream.ReadInt32();
                        barEntry.Stream.Seek(0x0, SeekOrigin.Begin);

                        switch (modelType)
                        {
                            //TODO: shadow models? i don't need them, but i presume if someone wants to make a tool targeting ps2 or pc port they'll need it
                            case 3:
                                skeletalModels.Add(ModelSkeletal.Read(barEntry.Stream));
                                break;
                        }
                        break;
                    }
                    case Bar.EntryType.ModelTexture:
                    {
                        textures.Add(ModelTexture.Read(barEntry.Stream));
                        break;
                    }
                    case Bar.EntryType.ModelCollision:
                    {
                        collisions.Add(barEntry.Stream.ReadAllBytes());
                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        var texImg = textures.First();


        var images = usesHdTextures
            ? hdTexs
            : texImg.Images
                .Select(texture => Image.CreateFromData(texture.Size.Width, texture.Size.Height, false, Image.Format.Rgba8, texture.ToBgra32().BGRAToRGBA()))
                .Select(ImageTexture.CreateFromImage)
                .ToList();

        if (skeletalModels.Count > 0)
        {
            //TODO
            var model = skeletalModels.First();

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
                }
                if (header.AlphaAdd)
                {
                    material.SetShaderParameter("Alpha", true);
                }
                if (header.AlphaSub)
                {
                    material.SetShaderParameter("Alpha", true);
                }
                if (header.AlphaEx)
                {
                    material.SetShaderParameter("Alpha", true);
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

                        positions.Add(new Vector3(pos.X, pos.Y, pos.Z) * ImportHelpers.KH2PositionScale);
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

                if (collisions.Count != 0)
                {
                    root.ModelCollisions = new ModelCollisionResource();
                    root.ModelCollisions.Binary = collisions.First();
                }

                root.Meshes.Add(mesh);
                root.Skeleton = skeleton;
            }
        }

        return root;
    }
    public static Node3D FromMap(Bar map, List<ImageTexture> hdTexs = null)
    {
        var usesHdTextures = hdTexs is not null;

        var hdTextureCount = 0;
        var hdTextureMap = new System.Collections.Generic.Dictionary<int, int>();

        var root = new Node3D();

        /*
        GD.Print(map.Count);
        foreach (var entry in map)
        {
            GD.Print($"{entry.Name}: {entry.Type}");
        }
        */

        var mainModel = map.FirstOrDefault(i => i.Name == "MAP" && i.Type == Bar.EntryType.Model);

        if (mainModel is null) return null;

        var mainModelTextures = map.FirstOrDefault(i => i.Name == "MAP" && i.Type == Bar.EntryType.ModelTexture);

        if (mainModelTextures is null) return null;

        var background = new ModelBackground(new MemoryStream(mainModel.Stream.ReadAllBytes().Skip(0x90).ToArray()));
        var texImg = ModelTexture.Read(mainModelTextures.Stream);

        var mesh = new MeshInstance3D();
        root.AddChild(mesh);

        var arrayMesh = new ArrayMesh();
        
        var images = usesHdTextures
            ? hdTexs
            : texImg.Images
                .Select(texture => Image.CreateFromData(texture.Size.Width, texture.Size.Height, false, Image.Format.Rgba8, texture.ToBgra32().BGRAToRGBA()))
                .Select(ImageTexture.CreateFromImage)
                .ToList();
        
        var indexBuffer = new int[4];
        var recentIndex = 0;

        var meshDictionary = new System.Collections.Generic.Dictionary<(int texIndex, bool alpha, bool alphaAdd, bool alphaSub), (List<Vector3> pos, List<Vector2> uv, List<Color> color)>();
        
        for (var m = 0; m < background.Chunks.Count; m++)
        {
            var chunk = background.Chunks[m];
            var texIndex = chunk.TextureId;

            var positions = new List<Vector3>();
            //var normals = new List<Vector3>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();

            var indices = new List<int>();

            var unpacker = new VifUnpacker(chunk.VifPacket);
            unpacker.Run();
            using var str = new MemoryStream(unpacker.Memory);

            var vpu = VpuPacket.Read(str);

            //var useNormal = vpu.Normals.Length > 0;
            var useColor = vpu.Colors.Length > 0;
            
            GD.Print(vpu.Indices.Length);
            
            var baseVertexIndex = positions.Count;
            foreach (var index in vpu.Indices)
            {
                var i = index.Index;
                var pos = vpu.Vertices[i];
                var indexInfo = vpu.Indices[i];

                positions.Add(new Vector3(pos.X, pos.Y, pos.Z) * ImportHelpers.KH2PositionScale);
                uvs.Add(new Vector2(indexInfo.U / 4096f, indexInfo.V / 4096f));

                /*
                if (useNormal)
                {
                    var normal = vpu.Normals[i];
                    normals.Add(new Vector3(normal.X, normal.Y, normal.Z));
                }
                else normals.Add(Vector3.One);
                */
                if (useColor)
                {
                    var color = vpu.Colors[i];
                    colors.Add(new Color(color.R / 128f, color.G / 128f, color.B / 128f, color.A / 128f));
                }
                else colors.Add(Colors.White);
                
                indexBuffer[(recentIndex++) & 3] = baseVertexIndex + i;
                switch (index.Function)
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
                    case VpuPacket.VertexFunction.DrawTriangleInverse:
                        indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                        indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                        indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                        break;
                    case VpuPacket.VertexFunction.DrawTriangle:
                        indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                        indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                        indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                        break;
                }
            }

            var dictIndex = (texIndex, chunk.IsAlpha, chunk.IsAlphaAdd, chunk.IsAlphaSubtract);

            if (!meshDictionary.TryGetValue(dictIndex, out var result))
            {
                result = ([], [], []);
                meshDictionary.Add(dictIndex, result);
            }
            result.pos.AddRange(indices.Select(i => positions[i]).ToArray());
            result.uv.AddRange(indices.Select(i => uvs[i]).ToArray());
            result.color.AddRange(indices.Select(i => colors[i]).ToArray());

            //var flags = Mesh.ArrayFormat.FormatVertex | Mesh.ArrayFormat.FormatTexUV;

            //if (useNormal) flags |= Mesh.ArrayFormat.FormatNormal;
            //if (useColor) flags |= Mesh.ArrayFormat.FormatColor; 
        }

        var mIndex = 0;

        foreach (var meshCollection in meshDictionary)
        {
            var texIndex = meshCollection.Key.texIndex;
            var alpha = meshCollection.Key.alpha;
            var alphaAdd = meshCollection.Key.alphaAdd;
            var alphaSub = meshCollection.Key.alphaSub;

            var pos = meshCollection.Value.pos;
            var uv = meshCollection.Value.uv;
            var color = meshCollection.Value.color;
            
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
            //var texSize = texImg.Images[texIndex].Size;
            var material = new ShaderMaterial();
            material.Shader = BasicShader;

            if (alpha)
            {
                material.SetShaderParameter("Alpha", true);
                material.SetShaderParameter("Scissor", true);
            }
            if (alphaAdd)
            {
                material.SetShaderParameter("Alpha", true);
            }
            if (alphaSub)
            {
                material.SetShaderParameter("Alpha", true);
            }
            material.SetShaderParameter("Texture", tex);
            
            var array = new Array();
            array.Resize((int)Mesh.ArrayType.Max);
            
            array[(int)Mesh.ArrayType.Vertex] = pos.ToArray();
            array[(int)Mesh.ArrayType.TexUV] = uv.ToArray();

            //array[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            array[(int)Mesh.ArrayType.Color] = color.ToArray();
            
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array, flags: Mesh.ArrayFormat.FormatVertex | Mesh.ArrayFormat.FormatTexUV | Mesh.ArrayFormat.FormatColor);
            arrayMesh.SurfaceSetMaterial(mIndex, material);
            mIndex++;
        }

        mesh.Name = "Background";
        mesh.Mesh = arrayMesh;
        mesh.Owner = root;

        return root;
    }
    public static SoundContainer FromScd(Scd scd)
    {
        var container = new SoundContainer();

        for (var index = 0; index < scd.StreamFiles.Count; index++)
        {
            var media = scd.MediaFiles[index];
            var header = scd.StreamHeaders[index];
            var streamFile = scd.StreamFiles[index];

            if (header.Codec == 6)
            {
                container.Sounds.Add(new SoundResource
                {
                    Sound = AudioStreamOggVorbis.LoadFromBuffer(media),
                    LoopStart = header.LoopStart,
                    LoopEnd = header.LoopEnd,
                    OriginalCodec = SoundResource.Codec.Ogg,
                });
            }
            else
            {
                //convert from MSADPCM to ogg, godot doesnt support MSADPCM wav files
                var output = new MemoryStream();
                FFMpegArguments
                    .FromPipeInput(new StreamPipeSource(new MemoryStream(media)))
                    .OutputToPipe(new StreamPipeSink(output), ffmpegOptions =>
                        ffmpegOptions.ForceFormat("ogg"))
                    .ProcessSynchronously();

                container.Sounds.Add(new SoundResource
                {
                    Sound = AudioStreamOggVorbis.LoadFromBuffer(output.GetBuffer()),
                    LoopStart = header.LoopStart,
                    LoopEnd = header.LoopEnd,
                    OriginalCodec = SoundResource.Codec.msadpcm,
                });
            }
        }

        return container;
    }
}
