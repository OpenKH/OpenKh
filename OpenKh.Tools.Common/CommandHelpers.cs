using System.Windows.Input;

namespace kh.tools.common
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
