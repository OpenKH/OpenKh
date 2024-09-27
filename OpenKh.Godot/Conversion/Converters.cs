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
using Environment = Godot.Environment;

namespace OpenKh.Godot.Conversion;

public static class Converters
{
    public static Node2D FromInterfaceSequence(Bar barFile, List<Texture2D> hdTexs = null)
    {
        var root = new Node2D();
        
        var names = barFile.Select(i => i.Name).Distinct().ToList();

        var mapper = new TextureMapper(hdTexs);

        foreach (var n in names)
        {
            var image = barFile.FirstOrDefault(i => i.Name == n && i.Type == Bar.EntryType.Imgd);
            var sequence = barFile.FirstOrDefault(i => i.Name == n && i.Type == Bar.EntryType.Seqd);

            if (image is null || sequence is null) continue;

            var img = Imgd.Read(image.Stream);
            var seq = Sequence.Read(sequence.Stream);

            var sequenceResource = FromSequence(seq, TextureConverters.FromIgmd(img), mapper.GetNextTexture(null));

            var sequenceNode = new KH2InterfaceSequencePlayer();

            sequenceNode.Sequence = sequenceResource;
            
            root.AddChild(sequenceNode);
            sequenceNode.Name = n;
        }

        return root;
    }
    public static KH2InterfaceSpriteSequence FromSequence(Sequence sequence, Texture2D nativeTex, Texture2D hdTex = null)
    {
        const float framerate = 60;
        
        var sequenceNode = new KH2InterfaceSpriteSequence();

        sequenceNode.Texture = hdTex ?? nativeTex;
                
        var nativeTexSize = nativeTex.GetSize();

        foreach (var spriteGroup in sequence.SpriteGroups)
        {
            var mesh = new ArrayMesh();

            var array = new Array();
            array.Resize((int)Mesh.ArrayType.Max);

            var positions = new List<Vector2>();
            var uvs = new List<Vector2>();
            var adjustments = new List<Vector2>();
            var miniMaxis = new List<Vector4>();
            var colors = new List<Color>();
            
            foreach (var spritePart in spriteGroup)
            {
                var sprite = sequence.Sprites[spritePart.SpriteIndex];

                var posTopLeft = new Vector2(spritePart.Left, spritePart.Top);
                var posTopRight = new Vector2(spritePart.Right, spritePart.Top);
                var posBottomLeft = new Vector2(spritePart.Left, spritePart.Bottom);
                var posBottomRight = new Vector2(spritePart.Right, spritePart.Bottom);

                var uvTopLeft = new Vector2(sprite.Left, sprite.Top);
                var uvTopRight = new Vector2(sprite.Right, sprite.Top);
                var uvBottomLeft = new Vector2(sprite.Left, sprite.Bottom);
                var uvBottomRight = new Vector2(sprite.Right, sprite.Bottom);

                var min = new Vector2(Mathf.Min(sprite.Left, sprite.Right), Mathf.Min(sprite.Top, sprite.Bottom)) / nativeTexSize;
                var max = new Vector2(Mathf.Max(sprite.Left, sprite.Right), Mathf.Max(sprite.Top, sprite.Bottom)) / nativeTexSize;

                var miniMaxi = new Vector4(min.X, min.Y, max.X, max.Y);
                
                var colorTopLeft = sprite.ColorLeft.ConvertColor();
                var colorTopRight = sprite.ColorTop.ConvertColor();
                var colorBottomLeft = sprite.ColorRight.ConvertColor();
                var colorBottomRight = sprite.ColorBottom.ConvertColor();

                var movement = new Vector2(sprite.UTranslation, sprite.VTranslation) * 60;
                        
                positions.AddRange(
                [
                    posTopLeft, posTopRight, posBottomLeft,
                    posBottomLeft, posTopRight, posBottomRight,
                ]);
                uvs.AddRange(
                [
                    uvTopLeft, uvTopRight, uvBottomLeft,
                    uvBottomLeft, uvTopRight, uvBottomRight,
                ]);
                adjustments.AddRange(
                [
                    movement, movement, movement,
                    movement, movement, movement,
                ]);
                miniMaxis.AddRange(
                [
                    miniMaxi, miniMaxi, miniMaxi,
                    miniMaxi, miniMaxi, miniMaxi,
                ]);
                colors.AddRange(
                [
                    colorTopLeft, colorTopRight, colorBottomLeft,
                    colorBottomLeft, colorTopRight, colorBottomRight,
                ]);
            }

            array[(int)Mesh.ArrayType.Vertex] = positions.ToArray();
            array[(int)Mesh.ArrayType.TexUV] = uvs.Select(u => u / nativeTexSize).ToArray();
            array[(int)Mesh.ArrayType.Color] = colors.ToArray();
            array[(int)Mesh.ArrayType.Custom0] = miniMaxis.SelectMany(i => new[] { i.X, i.Y, i.Z, i.W }).ToArray();
            array[(int)Mesh.ArrayType.Custom1] = adjustments.SelectMany(i => new[] { i.X, i.Y, }).ToArray();

            const Mesh.ArrayFormat miniMaxiShift = (Mesh.ArrayFormat)((int)Mesh.ArrayCustomFormat.RgbaFloat << (int)Mesh.ArrayFormat.FormatCustom0Shift);
            const Mesh.ArrayFormat adjustmentShift = (Mesh.ArrayFormat)((int)Mesh.ArrayCustomFormat.RgFloat << (int)Mesh.ArrayFormat.FormatCustom1Shift);
            
            if (spriteGroup.Count > 0)
                mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array,
                    flags: Mesh.ArrayFormat.FormatVertex | Mesh.ArrayFormat.FormatTexUV | Mesh.ArrayFormat.FormatColor | Mesh.ArrayFormat.FlagUse2DVertices | miniMaxiShift | adjustmentShift);
            
            sequenceNode.Sprites.Add(mesh);
        }
        
        foreach (var animationGroup in sequence.AnimationGroups)
        {
            var groupResource = new KH2InterfaceSequenceAnimationGroup();
            groupResource.Loop = animationGroup.DoNotLoop == 0;
            groupResource.LoopStart = animationGroup.LoopStart / framerate;
            groupResource.LoopEnd = animationGroup.LoopEnd / framerate;
            if (groupResource.Loop && groupResource.LoopEnd == 0) groupResource.LoopEnd = animationGroup.Animations.Max(i => i.FrameEnd) / framerate;
            
            foreach (var currentAnim in animationGroup.Animations)
            {
                var anim = new KH2InterfaceSequenceAnimation
                {
                    SpriteIndex = currentAnim.SpriteGroupIndex,
                    AnimationFlags = (InterfaceAnimationFlags)currentAnim.Flags,
                    TimeStart = currentAnim.FrameStart / framerate,
                    TimeEnd = currentAnim.FrameEnd / framerate,
                    TranslateStart = new Vector2(currentAnim.TranslateXStart, currentAnim.TranslateYStart),
                    TranslateEnd = new Vector2(currentAnim.TranslateXEnd, currentAnim.TranslateYEnd),
                    RotationStart = new Vector3(currentAnim.RotationXStart, currentAnim.RotationYStart, currentAnim.RotationZStart),
                    RotationEnd = new Vector3(currentAnim.RotationXEnd, currentAnim.RotationYEnd, currentAnim.RotationZEnd),
                    ScaleStart = new Vector2(currentAnim.ScaleXStart, currentAnim.ScaleYStart),
                    ScaleEnd = new Vector2(currentAnim.ScaleXEnd, currentAnim.ScaleYEnd),
                    ScaleUniformStart = currentAnim.ScaleStart,
                    ScaleUniformEnd = currentAnim.ScaleEnd,
                    PivotStart = new Vector2(currentAnim.PivotXStart, currentAnim.PivotYStart),
                    PivotEnd = new Vector2(currentAnim.PivotXEnd, currentAnim.PivotYEnd),
                    CurveStart = new Vector2(currentAnim.CurveXStart, currentAnim.CurveYStart),
                    CurveEnd = new Vector2(currentAnim.CurveXEnd, currentAnim.CurveYEnd),
                    BounceStart = new Vector2(currentAnim.BounceXStart, currentAnim.BounceYStart),
                    BounceEnd = new Vector2(currentAnim.BounceXEnd, currentAnim.BounceYEnd),
                    BounceCount = new Vector2I(currentAnim.BounceXCount, currentAnim.BounceYCount),
                    ColorBlend = currentAnim.ColorBlend,
                };

                anim.ColorStart = currentAnim.ColorStart.ConvertColor();
                anim.ColorEnd = currentAnim.ColorEnd.ConvertColor();
                
                groupResource.Animations.Add(anim);
            }
            sequenceNode.AnimationList.Add(groupResource);
        }
        return sequenceNode;
    }

    public static Node2D FromInterfaceLayout(Bar layout, List<Texture2D> hdTexs = null)
    {
        var mapper = new TextureMapper(hdTexs);
        
        var root = new KH2InterfaceLayoutPlayer();

        var imageEntry = layout.FirstOrDefault(i => i.Type == Bar.EntryType.Imgz);
        var layoutEntry = layout.FirstOrDefault(i => i.Type == Bar.EntryType.Layout);

        if (imageEntry is null || layoutEntry is null) return null;

        var nativeImages = new Imgz(imageEntry.Stream);
        var nativeImageList = nativeImages.Images.Select(TextureConverters.FromIgmd).ToList();

        for (var i = 0; i < nativeImageList.Count; i++) mapper.GetTexture(i, null);

        var nativeLayout = Layout.Read(layoutEntry.Stream);

        var layoutResource = new KH2InterfaceLayout();

        foreach (var group in nativeLayout.SequenceGroups)
        {
            var groupResource = new KH2InterfaceSequenceGroup();

            foreach (var sequence in group.Sequences)
            {
                var nativeTex = nativeImageList[sequence.TextureIndex];
                
                var sequenceResource = FromSequence(nativeLayout.SequenceItems[sequence.SequenceIndex], nativeTex, mapper.GetTexture(sequence.TextureIndex, null));

                groupResource.Sequences.Add(sequenceResource);
                groupResource.SequenceIndices.Add(sequence.AnimationGroup);
                groupResource.SequencePositions.Add(new Vector2(sequence.PositionX, sequence.PositionY));
                groupResource.ShowAtTime = sequence.ShowAtFrame / 60f;
            }
            
            layoutResource.Groups.Add(groupResource);
        }

        root.Layout = layoutResource;

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
