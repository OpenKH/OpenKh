#if TOOLS

using System;
using System.IO;
using System.Linq;
using FFMpegCore;
using FFMpegCore.Pipes;
using Godot;
using Godot.Collections;
using OpenKh.Bbs;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.addons.OpenKHImporter;

public partial class ScdImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }
    public override string _GetImporterName() => "ScdImporter";
    public override string _GetVisibleName() => "KH Sound Container Importer";
    public override string[] _GetRecognizedExtensions() => ["scd"];
    public override string _GetSaveExtension() => "tres";
    public override string _GetResourceType() => "SoundContainer";
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

        var scd = Scd.Read(stream);

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

        ResourceSaver.Save(container, $"{savePath}.{_GetSaveExtension()}");

        return Error.Ok;
    }
    private static void Decrypt(byte[] file, int encryptionKey, int startingPosition, int endPosition)
    {
        var key = BitConverter.GetBytes(encryptionKey);
        for (var i = startingPosition; i < endPosition; i++) file[i] = (byte)(file[i]^key[0]);
    }
}

#endif
