using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Common.Wpf
{
    public class SimpleAsyncActionCommand<T> : ICommand
    {
        private readonly Func<T, Task> _asyncAction;
        private readonly Action<Task> _newTask;
        private Task _task = null;
        public bool _isEnabled = true;

        public event EventHandler CanExecuteChanged;
        public event PropertyChangedEventHandler PropertyChanged;

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
