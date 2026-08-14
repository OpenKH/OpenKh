namespace OpenKh.Tools.ModsManager.Core.Interfaces
{
    public interface IDebugging
    {
        void HideDebugger();
        void Log(long ms, string tag, string str);
    }
}
