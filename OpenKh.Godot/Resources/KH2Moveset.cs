using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Kh2;

namespace OpenKh.Godot.Resources;

[Tool, GlobalClass]
public partial class KH2Moveset : Resource
{
    [Export] public Array<byte[]> RawAnimations = new();
    public List<AnimationBinary> AnimationBinaries => _binaries ??= RawAnimations.Select(i => i is not null && i.Length > 0 ? new AnimationBinary(new MemoryStream(i)) : null).ToList();
    private List<AnimationBinary> _binaries;
}
