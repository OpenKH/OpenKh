using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace OpenKh.Tools.Common
{
    public static class Utilities
    {
        public static readonly Action<Exception> DefaultExceptionHandler = ex =>
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        public static Action<Exception> ExceptionHandler = DefaultExceptionHandler;
        
        public static Window GetCurrentWindow() =>
            Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

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
        
        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(GetCurrentWindow(), message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
