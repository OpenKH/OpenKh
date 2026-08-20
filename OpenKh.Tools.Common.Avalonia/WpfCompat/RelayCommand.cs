// Compatibility reimplementation of Xe.Tools.Wpf.Commands.RelayCommand
// (from the XeEngine.Tools.Public submodule) for the Avalonia builds.
// Declared in the same namespace so shared sources compile unchanged;
// only Avalonia executables reference this assembly.
//
// The WPF original never raises CanExecuteChanged itself. It relies on
// WPF's CommandManager.RequerySuggested global requery, which Avalonia does
// not have. This version keeps a static requery event with the same role:
// executing any RelayCommand triggers a global requery, and the application
// can call InvalidateRequerySuggested() from other state-change points.

using System;
using System.Windows.Input;
using Avalonia.Threading;

namespace Xe.Tools.Wpf.Commands
{
    public class RelayCommand : ICommand
    {
        private static event EventHandler RequerySuggested;

        public static void InvalidateRequerySuggested()
        {
            if (Dispatcher.UIThread.CheckAccess())
                RequerySuggested?.Invoke(null, EventArgs.Empty);
            else
                Dispatcher.UIThread.Post(() => RequerySuggested?.Invoke(null, EventArgs.Empty));
        }

        private readonly Action<object> _execute, _undo;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { RequerySuggested += value; }
            remove { RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute,
            Func<object, bool> canExecute = null,
            Action<object> undo = null)
        {
            _execute = execute;
            _undo = undo;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
            InvalidateRequerySuggested();
        }

        public void Undo(object parameter)
        {
            _undo(parameter);
        }
    }
}
