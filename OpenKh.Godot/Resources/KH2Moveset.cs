using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool, GlobalClass]
public partial class KH2Moveset : Resource
{
    [Export] public Array<Animation> Animations = new();
    public AnimationLibrary AnimLib
    {
        get
        {
            var lib = new AnimationLibrary();
            for (var i = 0; i < Animations.Count; i++)
            {
                var anim = Animations[i];
                if (anim is null) continue;
                lib.AddAnimation($"{i}", anim);
            }
            return lib;
        }
    }
}
