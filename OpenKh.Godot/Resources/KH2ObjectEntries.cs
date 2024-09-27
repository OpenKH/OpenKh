using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources
{
    public partial class KH2ObjectEntries : Resource
    {
        [Export] public Array<KH2ObjectEntry> Entries = new();
    }
}
