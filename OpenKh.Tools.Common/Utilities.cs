using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace OpenKh.Tools.Common
{
    public static class Utilities
    {
        private static readonly Assembly RunningAssembly = Assembly.GetEntryAssembly();
        private static readonly AssemblyName RunningAssemblyName = RunningAssembly?.GetName();

        public static readonly Action<Exception> DefaultExceptionHandler = ex =>
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        public static Action<Exception> ExceptionHandler = DefaultExceptionHandler;

        public static Window GetCurrentWindow() =>
            Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        public static string GetApplicationName()
        {
            var fvi = FileVersionInfo.GetVersionInfo(RunningAssembly.Location);
            return fvi.ProductName;
        }

        public static string GetApplicationVersion()
        {
            var version = RunningAssemblyName?.Version;
            if (version == null)
                return "unknown";
            return $"{version.Major}.{version.Minor:D02}.{version.Build:D02}.{version.Revision}";
        }

        public static void Catch(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(GetCurrentWindow(), message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
