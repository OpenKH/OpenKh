using System.IO;
using FFMpegCore;
using FFMpegCore.Pipes;
using Godot;
using OpenKh.Bbs;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Conversion;

public static class Converters
{
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
