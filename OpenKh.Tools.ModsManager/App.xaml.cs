using OpenKh.Common;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace OpenKh.Tools.ModsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Capture unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string errorMessage = $"Unhandled exception ({source}):\n{exception}";
            MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Save the error to a log file
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                File.AppendAllText(logPath, $"[{DateTime.Now}] {errorMessage}\n\n");
            }
            catch
            {
                // Do nothing if we can't save the log
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Close();
            base.OnExit(e);
        }
    }
}
