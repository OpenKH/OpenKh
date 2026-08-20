// Compatibility reimplementation of OpenKh.Tools.Common.Wpf's Utilities,
// RelayCommand<T> and SimpleAsyncActionCommand<T> for the Avalonia builds.
// Declared in the same namespace so shared sources compile unchanged; only
// Avalonia executables reference this assembly.

using OpenKh.Tools.Common.Avalonia.Internal;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace OpenKh.Tools.Common.Wpf
{
    public static class Utilities
    {
        private static readonly Assembly RunningAssembly = Assembly.GetEntryAssembly();
        private static readonly AssemblyName RunningAssemblyName = RunningAssembly?.GetName();

        public static readonly Action<Exception> DefaultExceptionHandler = ex =>
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        public static Action<Exception> ExceptionHandler = DefaultExceptionHandler;

        public static AvaloniaWindow GetCurrentWindow() =>
            WindowLocator.GetActiveWindow();

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
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public class RelayCommand<T> : Xe.Tools.Wpf.Commands.RelayCommand
    {
        public RelayCommand(Action<T> execute,
            Func<T, bool> canExecute = null,
            Action<T> undo = null) :
            base(x => execute((T)x), CanExecute(canExecute), Undo(undo))
        { }

        private static Func<object, bool> CanExecute(Func<T, bool> canExecute)
        {
            if (canExecute == null)
                return null;
            return x => canExecute((T)x);
        }

        private static Action<object> Undo(Action<T> undo)
        {
            if (undo == null)
                return null;
            return x => undo((T)x);
        }
    }

    public class SimpleAsyncActionCommand<T> : ICommand
    {
        private readonly Func<T, Task> _asyncAction;
        private readonly Action<Task> _newTask;
        private Task _task = null;
        public bool _isEnabled = true;

        public event EventHandler CanExecuteChanged;
        public SimpleAsyncActionCommand(
            Func<T, Task> asyncAction,
            Action<Task> newTask = null
        )
        {
            _asyncAction = asyncAction;
            _newTask = newTask;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return _isEnabled && (_task == null || _task.IsCompleted);
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                async Task AwaitAsync(Task task)
                {
                    _task = task;
                    _newTask?.Invoke(_task);
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);

                    try
                    {
                        await task;
                    }
                    finally
                    {
                        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    }
                }

                var task = AwaitAsync(_asyncAction((T)parameter));
            }
        }
    }
}
