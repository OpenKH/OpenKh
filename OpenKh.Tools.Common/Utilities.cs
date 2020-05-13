using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace OpenKh.Tools.Common
{
    public static class Utilities
    {
        public static readonly Action<Exception> DefaultExceptionHandler = ex =>
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        public static Action<Exception> ExceptionHandler = DefaultExceptionHandler;

        public static string GetApplicationName()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductName;
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
    }
}
