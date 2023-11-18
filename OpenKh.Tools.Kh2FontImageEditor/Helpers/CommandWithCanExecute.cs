using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2FontImageEditor.Helpers
{
    public class CommandWithCanExecute : ICommand
    {
        public CommandWithCanExecute(Action<object?> action)
        {
            _action = action;
        }

        private bool _isExecutable;
        private readonly Action<object?> _action;

        public bool IsExecutable
        {
            get => _isExecutable;
            set
            {
                _isExecutable = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _isExecutable;
        }

        public void Execute(object? parameter)
        {
            _action(parameter);
        }
    }
}
