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
    [Export] public Array<AnimationBinaryResource> AnimationBinaries = new();
}
