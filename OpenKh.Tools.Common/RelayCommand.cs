using System;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.Common
{
    public class RelayCommand<T> : RelayCommand
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
}
