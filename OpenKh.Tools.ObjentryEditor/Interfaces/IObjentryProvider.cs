using System.Collections.Generic;

namespace OpenKh.Tools.ObjentryEditor.Interfaces
{
    public interface IObjentryProvider
    {
        IEnumerable<IObjentryEntry> ObjentryEntries { get; }

        bool DoesObjentryExists(ushort itemId);
        string GetObjName(ushort itemId);
    }
}
