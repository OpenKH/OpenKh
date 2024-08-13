#if TOOLS

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Godot;
using Godot.Collections;
using OpenKh.Common;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2Anim.Mset;
using Quaternion = Godot.Quaternion;
using Vector3 = Godot.Vector3;

namespace OpenKh.Godot.addons.OpenKHImporter;

//TODO: this takes wayyyy too long to import one file
public partial class MsetImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }
    public override string _GetImporterName() => "MsetImporter";
    public override string _GetVisibleName() => "KH2 Moveset Importer";
    public override string[] _GetRecognizedExtensions() => ["mset"];
    public override string _GetSaveExtension() => "tres";
    public override string _GetResourceType() => "KH2Moveset";
    public override float _GetPriority() => 1;
    public override int _GetImportOrder() => 0;
    public override int _GetPresetCount() => Enum.GetValues<Presets>().Length;
    public override string _GetPresetName(int presetIndex) => ((Presets)presetIndex).ToString();
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => base._GetImportOptions(path, presetIndex);

    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        var realPath = ProjectSettings.GlobalizePath(sourceFile);
        
        using var stream = File.Open(realPath, FileMode.Open, System.IO.FileAccess.Read);

        if (!BinaryArchive.IsValid(stream)) return Error.InvalidData;
        
        var msetBinarc = BinaryArchive.Read(stream);

        var mdlxPath = realPath.Replace(".mset", ".mdlx");

        using var mdlx = File.Open(mdlxPath, FileMode.Open, System.IO.FileAccess.Read);

        var mdlxBytes = mdlx.ReadAllBytes();

        var skeleton = ModelSkeletal.Read(Bar.Read(new MemoryStream(mdlxBytes)).First(i => i.Type == Bar.EntryType.Model).Stream).Bones;
        
        var container = new KH2Moveset();

        foreach (var entry in msetBinarc.Entries)
        {
            var animation = new Animation();
            
            GD.Print(entry.Link);

            if (entry.Link < 0)
            {
                container.Animations.Add(null);
                continue;
            }
            
            using var fileStream = new MemoryStream(msetBinarc.Subfiles[entry.Link]);
            var loadedMotion = new AnimationBinary(fileStream);
            
            var anbBarFile = Bar.Read(new MemoryStream(msetBinarc.Subfiles[entry.Link]));
            
            var loadedAnb = new AnbIndir(anbBarFile);
            
            var animMatricesProvider = loadedAnb.GetAnimProvider(mdlx);
            
            var MotionMinFrame = (int)loadedMotion.MotionFile.InterpolatedMotionHeader.FrameData.FrameStart * 2;
            var MotionMaxFrame = (int)loadedMotion.MotionFile.InterpolatedMotionHeader.FrameData.FrameEnd * 2;
            
            //var CurrentFrame = MotionMinFrame;

            var boneCount = animMatricesProvider.MatrixCount;
            
            for(var vert = 0; vert < boneCount; vert++)
            {
                animation.AddTrack(Animation.TrackType.Position3D, (vert * 3));
                animation.AddTrack(Animation.TrackType.Rotation3D, (vert * 3) + 1);
                animation.AddTrack(Animation.TrackType.Scale3D, (vert * 3) + 2);
                
                animation.TrackSetPath((vert * 3), $"Skeleton:{vert}");
                animation.TrackSetPath((vert * 3) + 1, $"Skeleton:{vert}");
                animation.TrackSetPath((vert * 3) + 2, $"Skeleton:{vert}");
            }

            var delta = 1 / animMatricesProvider.FramePerSecond;

            for (var j = MotionMinFrame; j < MotionMaxFrame; j++)
            {
                //TODO: if im reading it right, this is the culprit
                var matrices = animMatricesProvider.ProvideMatrices(j);

                var time = delta * j;

                for (var m = 0; m < matrices.Length; m++)
                {
                    var parent = skeleton[m].ParentIndex;
                    var matrix = matrices[m];

                    if (parent is not 1023 and not -1)
                    {
                        var parentMatrix = matrices[parent];
                        
                        Matrix4x4.Decompose(matrix, out var scale, out var rot, out var pos);
                        Matrix4x4.Decompose(parentMatrix, out var pscale, out var prot, out var ppos);

                        var godotMain = new Transform3D(new Basis(new Quaternion(rot.X, rot.Y, rot.Z, rot.W)).Scaled(new Vector3(scale.X, scale.Y, scale.Z)), new Vector3(pos.X, pos.Y, pos.Z));
                        var godotParent = new Transform3D(new Basis(new Quaternion(prot.X, prot.Y, prot.Z, prot.W)).Scaled(new Vector3(pscale.X, pscale.Y, pscale.Z)), new Vector3(ppos.X, ppos.Y, ppos.Z));

                        var relative = godotParent.Inverse() * godotMain;
                        
                        animation.PositionTrackInsertKey((m * 3), time, relative.Origin * Helpers.KH2PositionScale);
                        animation.RotationTrackInsertKey((m * 3) + 1, time, relative.Basis.GetRotationQuaternion());
                        animation.ScaleTrackInsertKey((m * 3) + 2, time, relative.Basis.Scale);
                    }
                    else
                    {
                        Matrix4x4.Decompose(matrix, out var scale, out var rot, out var pos);

                        animation.PositionTrackInsertKey((m * 3), time, new Vector3(pos.X, pos.Y, pos.Z) * Helpers.KH2PositionScale);
                        animation.RotationTrackInsertKey((m * 3) + 1, time, new Quaternion(rot.X, rot.Y, rot.Z, rot.W));
                        animation.ScaleTrackInsertKey((m * 3) + 2, time, new Vector3(scale.X, scale.Y, scale.Z));
                    }
                    

                }
            }
            container.Animations.Add(animation);
        }
        
        ResourceSaver.Save(container, $"{savePath}.{_GetSaveExtension()}");

        return Error.Ok;
    }
}

#endif
