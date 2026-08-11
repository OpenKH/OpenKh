using OpenKh.Tools.ModsManager.Interfaces;

namespace OpenKh.Tools.ModsManager.Core;

internal sealed class NullDebugging : IDebugging
{
    public void HideDebugger()
    {
    }

    public void Log(long ms, string tag, string str)
    {
    }
}
