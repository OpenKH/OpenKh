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
    //KH2 entity shaders
    private static Shader BasicShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2BasicShader.gdshader");
    private static Shader AnimatedShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2AnimatedShader.gdshader");
    
    //KH2 world shaders
    private static Shader WorldOpaqueShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2WorldOpaqueShader.gdshader");
    private static Shader WorldAlphaMixShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2WorldAlphaMixShader.gdshader");
    private static Shader WorldAlphaAddShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2WorldAlphaAddShader.gdshader");
    private static Shader WorldAlphaSubShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2WorldAlphaSubShader.gdshader");
    private static Shader WorldAlphaMulShader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2WorldAlphaMulShader.gdshader");

    public static KH2Mdlx FromMdlx(Bar bar, List<ImageTexture> hdTexs = null)
    {
        var usesHdTextures = hdTexs is not null;

        var root = new KH2Mdlx();

        var skeletalModels = new List<ModelSkeletal>();
        var textures = new List<ModelTexture>();
        var collisions = new List<byte[]>();

        foreach (var barEntry in bar)
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

        var mapper = new TextureMapper(usesHdTextures ? hdTexs : null);
        
        var images = texImg.Images
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
                //var tex = images[texIndex];

                var tex = mapper.GetTexture(texIndex, images[texIndex]);
                /*
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
                */
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
                        
                        var data = ImageDataHelpers.FromIndexed8ToBitmap32(animation.SpriteImage, texImg.Images[texIndex].GetClut(), ImageDataHelpers.RGBA).BGRAToRGBA();

                        var sprite = Image.CreateFromData(animation.SpriteWidth, animation.SpriteHeight * animation.NumSpritesInImageData, false, Image.Format.Rgba8, data);

                        var animatedTex = mapper.GetNextTexture(ImageTexture.CreateFromImage(sprite));
                        
                        /*
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
                        */

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

    public static Node3D FromCoct(Coct collision)
    {
        var root = new Node3D();
        for (var index = 0; index < collision.Nodes.Count; index++)
        {
            var node = collision.Nodes[index];
            var body = new Node3D();
            
            root.AddChild(body);
            
            body.Name = $"Node_{index}";

            for (var index1 = 0; index1 < node.Meshes.Count; index1++)
            {
                var mesh = node.Meshes[index1];

                var meshNode = new Node3D();
                body.AddChild(meshNode);

                meshNode.Name = $"Mesh_{index1}";

                var collisions = new System.Collections.Generic.Dictionary<int, ConcavePolygonShape3D>();
                
                //staticBody.CollisionLayer = mesh.Group; //TODO
                
                foreach (var c in mesh.Collisions)
                {
                    var flags = c.Attributes.Flags;

                    if (!collisions.TryGetValue(flags, out var shape))
                    {
                        var staticBody = new StaticBody3D();
                        meshNode.AddChild(staticBody);
                        staticBody.Name = $"Collisions_{flags:B16}";
                        staticBody.CollisionLayer = (uint)flags;
                        
                        var bodyShape = new CollisionShape3D();
                        staticBody.AddChild(bodyShape);
                        bodyShape.Name = "Shape";
                    
                        shape = new ConcavePolygonShape3D();
                        shape.SetBackfaceCollisionEnabled(true);
                        
                        bodyShape.Shape = shape;
                        
                        collisions.Add(flags, shape);
                    }
                    
                    var one = collision.VertexList[c.Vertex1];
                    var two = collision.VertexList[c.Vertex2];
                    var three = collision.VertexList[c.Vertex3];
                    
                    var vec1 = new Vector3(one.X, -one.Y, -one.Z) * ImportHelpers.KH2PositionScale;
                    var vec2 = new Vector3(two.X, -two.Y, -two.Z) * ImportHelpers.KH2PositionScale;
                    var vec3 = new Vector3(three.X, -three.Y, -three.Z) * ImportHelpers.KH2PositionScale;

                    var quad = collision.VertexList.Count > c.Vertex4 && c.Vertex4 >= 0;

                    if (quad)
                    {
                        var four = collision.VertexList[c.Vertex4];
                        var vec4 = new Vector3(four.X, -four.Y, -four.Z) * ImportHelpers.KH2PositionScale;
                        
                        shape.Data = shape.Data.Concat(new[]
                        {
                            vec1, vec2, vec3,
                            vec1, vec3, vec4,
                        }).ToArray();
                    }
                    else
                    {
                        shape.Data = shape.Data.Concat(new[]
                        {
                            vec1, vec2, vec3,
                        }).ToArray();
                    }
                }
            }
        }
        return root;
    }

    private enum AlphaType
    {
        Mix,
        Add,
        Sub,
        Mul,
        Scissor,
        Opaque
    }
    
    public static Node3D FromMap(Bar map, List<ImageTexture> hdTexs = null)
    {
        foreach (var entry in map)
        {
            GD.Print($"{entry.Name}, {entry.Type}");
        }
        
        var usesHdTextures = hdTexs is not null;

        var root = new Node3D();

        var mainModel = map.FirstOrDefault(i => i.Name == "MAP" && i.Type == Bar.EntryType.Model);

        if (mainModel is null) return null;

        var mainModelTextures = map.FirstOrDefault(i => i.Name == "MAP" && i.Type == Bar.EntryType.ModelTexture);

        if (mainModelTextures is null) return null;

        var background = new ModelBackground(new MemoryStream(mainModel.Stream.ReadAllBytes().Skip(0x90).ToArray()));
        var texImg = ModelTexture.Read(mainModelTextures.Stream);

        var mesh = new MeshInstance3D();
        root.AddChild(mesh);

        var arrayMesh = new ArrayMesh();

        var mapper = new TextureMapper(usesHdTextures ? hdTexs : null);
        
        var images = texImg.Images
                .Select(texture => Image.CreateFromData(texture.Size.Width, texture.Size.Height, false, Image.Format.Rgba8, texture.ToBgra32().BGRAToRGBA()))
                .Select(ImageTexture.CreateFromImage)
                .ToList();
        
        var meshDictionary = new System.Collections.Generic.Dictionary<(int texIndex, AlphaType alpha, int uvsc), (List<Vector3> pos, List<Color> color, List<Vector2> uvs)>();
        
        var maxId = background.Chunks.Max(i => i.TextureId);
        
        for (var i = 0; i <= maxId; i++) mapper.GetTexture(i, null);
        
        for (var m = 0; m < background.Chunks.Count; m++)
        {
            var chunk = background.Chunks[m];
            var texIndex = chunk.TextureId;
            
            //GD.Print($"{chunk.UVScrollIndex:B8}, {texIndex}");

            var alphaType = AlphaType.Opaque;
            if (chunk.IsAlphaAdd) alphaType = AlphaType.Add;
            else if (chunk.IsAlphaSubtract) alphaType = AlphaType.Sub;
            //else if (chunk.IsMulti) alphaType = AlphaType.Mul;
            else if (chunk.IsAlpha) alphaType = AlphaType.Mix;
            
            var dictIndex = (texIndex, alphaType, chunk.UVScrollIndex);

            if (!meshDictionary.TryGetValue(dictIndex, out var result))
            {
                result = ([], [], []);
                meshDictionary.Add(dictIndex, result);
            }
            
            var vertices = new List<(Vector3 pos, Vector2 uv, Color color)>();

            var unpacker = new VifUnpacker(chunk.VifPacket);
            while (unpacker.Run() != VifUnpacker.State.End)
            {
                var currentIndex = vertices.Count;
                
                using var str = new MemoryStream(unpacker.Memory);

                var vpu = VpuPacket.Read(str);
                var useColor = vpu.Colors.Length > 0;
                
                var colors = useColor ? 
                    vpu.Colors.Select(i => new Color(i.R / 128f, i.G / 128f, i.B / 128f, i.A / 128f)).ToList() : 
                    Enumerable.Repeat(new Color(0.5f, 0.5f, 0.5f), vpu.Indices.Length).ToList();
                
                for (var i = 0; i < vpu.Indices.Length; i++)
                {
                    var ind = vpu.Indices[i];
                    var pos = vpu.Vertices[ind.Index];
                    vertices.Add((new Vector3(pos.X, pos.Y, pos.Z) * ImportHelpers.KH2PositionScale, new Vector2((short)(ushort)ind.U, (short)(ushort)ind.V) / 4096f, colors[i]));
                }

                var indices = vpu.Indices;
                
                var resultPos = new List<Vector3>();
                var resultColor = new List<Color>();
                var resultUv = new List<Vector2>();

                for (var i = 0; i < vpu.Indices.Length; i++)
                {
                    var index = indices[i];
                    var func = index.Function;

                    var cur = currentIndex + i;
                    
                    if (func is VpuPacket.VertexFunction.DrawTriangle or VpuPacket.VertexFunction.DrawTriangleDoubleSided)
                    {
                        var first = cur - 1;
                        var second = cur - 2;
                        var third = cur;
                    
                        resultPos.AddRange([ vertices[first].pos, vertices[second].pos, vertices[third].pos ]);
                        resultUv.AddRange([ vertices[first].uv, vertices[second].uv, vertices[third].uv ]);
                        resultColor.AddRange([ vertices[first].color, vertices[second].color, vertices[third].color ]);
                    }
                    if (func is VpuPacket.VertexFunction.DrawTriangleInverse or VpuPacket.VertexFunction.DrawTriangleDoubleSided)
                    {
                        var first = cur - 2;
                        var second = cur - 1;
                        var third = cur;
                    
                        resultPos.AddRange([ vertices[first].pos, vertices[second].pos, vertices[third].pos ]);
                        resultUv.AddRange([ vertices[first].uv, vertices[second].uv, vertices[third].uv ]);
                        resultColor.AddRange([ vertices[first].color, vertices[second].color, vertices[third].color ]);
                    }
                }
                
                result.pos.AddRange(resultPos);
                result.color.AddRange(resultColor);
                result.uvs.AddRange(resultUv);
            }
        }

        var mIndex = 0;

        foreach (var meshCollection in meshDictionary)
        {
            var texIndex = meshCollection.Key.texIndex;
            var alpha = meshCollection.Key.alpha;
            var uvsc = meshCollection.Key.uvsc;
            
            //GD.Print($"{texIndex}, {uvsc:B8}");
            var nativeImage = images[texIndex];
            
            var tex = mapper.GetTexture(texIndex, nativeImage);
            
            //var texSize = texImg.Images[texIndex].Size;
            var material = new ShaderMaterial();

            material.Shader = alpha switch
            {
                AlphaType.Mix => WorldAlphaMixShader,
                AlphaType.Add => WorldAlphaAddShader,
                AlphaType.Sub => WorldAlphaSubShader,
                AlphaType.Mul => WorldAlphaMulShader,
                _ => WorldOpaqueShader,
            };
            
            //var uvX = 
            
            if (uvsc > 0 && uvsc < texImg.TextureFooterData.UvscList.Count)
            {
                var nativeSize = nativeImage.GetSize();
                GD.Print($"{uvsc}, {texImg.TextureFooterData.UvscList.Count}");
                
                var findUvsc = texImg.TextureFooterData.UvscList[uvsc];
                
                var uSpeed = (findUvsc.UScrollSpeed / 20000f) / nativeSize.X;
                var vSpeed = (findUvsc.VScrollSpeed / 20000f) / nativeSize.Y;
                
                material.SetShaderParameter("UVScrollSpeed", new Vector2(uSpeed, vSpeed));
            }
            
            /*
            var findUvsc = texImg.TextureFooterData.UvscList.FirstOrDefault(i => i.TextureIndex == texIndex);
            if (findUvsc is not null)
            {
                var nativeSize = nativeImage.GetSize();
                
                var uSpeed = (findUvsc.UScrollSpeed / 20000f) / nativeSize.X;
                var vSpeed = (findUvsc.VScrollSpeed / 20000f) / nativeSize.Y;
                
                GD.Print($"{uvsc}, {texImg.TextureFooterData.UvscList.Count} | {findUvsc.UScrollSpeed}, {findUvsc.VScrollSpeed} | {uSpeed}, {vSpeed}");
                
                material.SetShaderParameter("UVScrollSpeed", new Vector2(uSpeed, vSpeed));
            }
            */
            
            material.SetShaderParameter("Texture", tex);
            
            var array = new Array();
            array.Resize((int)Mesh.ArrayType.Max);
            
            array[(int)Mesh.ArrayType.Vertex] = meshCollection.Value.pos.ToArray();
            array[(int)Mesh.ArrayType.TexUV] = meshCollection.Value.uvs.ToArray();
            array[(int)Mesh.ArrayType.Color] = meshCollection.Value.color.ToArray();
            
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array, flags: Mesh.ArrayFormat.FormatVertex | Mesh.ArrayFormat.FormatTexUV | Mesh.ArrayFormat.FormatColor);
            arrayMesh.SurfaceSetMaterial(mIndex, material);
            mIndex++;
        }

        mesh.Name = "Background";
        mesh.Mesh = arrayMesh;
        //mesh.Owner = root;

        var collision = map.FirstOrDefault(i => i.Type == Bar.EntryType.CollisionOctalTree);

        if (collision is not null)
        {
            var coll = FromCoct(Coct.Read(collision.Stream));
            root.AddChild(coll);
            coll.Name = "Collision";
            //coll.Owner = root;
        }

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
