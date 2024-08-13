using Godot;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class SoundResource : Resource
{
    public enum Codec
    {
        Ogg,
        msadpcm,
    }
    [Export] public AudioStream Sound;
    [Export] public long LoopStart;
    [Export] public long LoopEnd;
    [Export] public Codec OriginalCodec;
}
