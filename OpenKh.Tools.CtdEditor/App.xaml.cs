using OpenKh.Tools.CtdEditor.ViewModels;
using OpenKh.Tools.CtdEditor.Views;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenKh.Tools.CtdEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                CrashHandler((Exception)e.ExceptionObject, "AppDomain");
            };

            DispatcherUnhandledException += (s, e) =>
            {
                CrashHandler(e.Exception, "Dispatcher");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                CrashHandler(e.Exception, "TaskScheduler");
                e.SetObserved();
            };
        }

        private void CrashHandler(Exception ex, string source)
        {
            using (var fs = File.Open(".\\Crash.log", FileMode.Create, FileAccess.Write))
            {
                using var writer = new StreamWriter(fs);

                writer.WriteLine("==== OpenKH Crash Log ====");
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                writer.WriteLine($"Assembly: {assemblyName.Name} {assemblyName.Version}");
                writer.WriteLine($"Time: {DateTime.Now}");
                writer.WriteLine();
                writer.WriteLine($"Exception Type: {ex.GetType().FullName}");
                writer.WriteLine($"Exception Details: {ex}");
                writer.WriteLine($"Caught From: {source}");
                writer.WriteLine();
                writer.WriteLine($"Command Line: {Environment.CommandLine}");
                writer.WriteLine($"CWD: {Environment.CurrentDirectory}");
                writer.WriteLine($"OS: {Environment.OSVersion}");
                writer.WriteLine($"{(Environment.Is64BitOperatingSystem ? "64 Bit OS" : "32 Bit OS")} {(Environment.Is64BitProcess ? "64 Bit Proc" : "32 Bit Proc")}");
                writer.WriteLine($"CLR: {Environment.Version}");
                writer.WriteLine($"CPUs: {Environment.ProcessorCount}");
                var gcinfo = GC.GetGCMemoryInfo();
                writer.WriteLine($"Working Set: {Environment.WorkingSet} GC Info: [HEAP {gcinfo.HeapSizeBytes} COMMIT {gcinfo.TotalCommittedBytes} AVAIL {gcinfo.TotalAvailableMemoryBytes}]");
                writer.WriteLine($"App specific info follows:");
                {
                    MainWindow ourWindow = (MainWindow)this.MainWindow;
                    MainViewModel viewModel = (MainViewModel)ourWindow.DataContext;
                    writer.WriteLine($"    Open file: {viewModel.FileName}");
                    writer.WriteLine($"    Open font: {viewModel.FontName}");
                }
                if (ex.Data.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Exception specific data follows:");
                    foreach (DictionaryEntry de in ex.Data)
                        writer.WriteLine($"{de.Key} : {de.Value}");
                }
                writer.WriteLine();
            }

            MessageBox.Show(MainWindow, $"Fatal Error: {ex.Message}\nPlease give Crash.log to the devs!", "OpenKH Has Crashed!", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.FailFast(null);
        }
    }
}
