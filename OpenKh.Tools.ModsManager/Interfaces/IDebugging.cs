namespace OpenKh.Tools.ModManager.Interfaces
{
    public interface IDebugging
    {
        void HideDebugger();
        void Log(long ms, string tag, string str);
    }
}
