using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MapStudio.Interfaces
{
    interface IObjEntryController
    {
        IEnumerable<Objentry> ObjectEntries { get; }

        MeshGroup this[int objId] { get; }
        MeshGroup this[string objName] { get; }

        string GetName(int objectId);
    }
}
