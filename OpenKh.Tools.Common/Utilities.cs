using System.Diagnostics;
using System.Reflection;

namespace kh.tools.common
{
    public static class Utilities
    {
        public static string GetApplicationName()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductName;
        }
    }
}
