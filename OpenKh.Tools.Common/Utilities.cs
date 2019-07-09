using System.Diagnostics;
using System.Reflection;

namespace OpenKh.Tools.Common
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
