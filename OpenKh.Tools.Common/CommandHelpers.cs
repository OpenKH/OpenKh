using System.Windows.Input;

namespace OpenKh.Tools.Common
{
    public static class CommandHelpers
    {
        public static bool Invoke(this ICommand command, object parameter)
        {
            if (!(command?.CanExecute(parameter) ?? true))
                return false;

            command?.Execute(parameter);
            return true;
        }
    }
}
