using Godot;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Test;

[Tool, GlobalClass]
public partial class TestAnimationLoader : AnimationPlayer
{
    [Export] public KH2Moveset Moveset;

    private bool _addedLib;

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        GD.Print("A");
        
        if (_addedLib || Moveset is null) return;
        
        GD.Print("DING DONG");
        
        _addedLib = true;
        var lib = Moveset.AnimLib;
        AddAnimationLibrary("ANIMSTEST", lib);
        Play("ANIMSTEST/11");
    }
}
