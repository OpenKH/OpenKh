// WPF compatibility shim: CommandManager.InvalidateRequerySuggested forwards
// to the compat RelayCommand's global requery event.
// See WpfEnums.cs for why this lives in a WPF namespace.

namespace System.Windows.Input
{
    public static class CommandManager
    {
        public static void InvalidateRequerySuggested() =>
            Xe.Tools.Wpf.Commands.RelayCommand.InvalidateRequerySuggested();
    }
}
