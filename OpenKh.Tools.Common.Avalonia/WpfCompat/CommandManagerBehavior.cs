// Emulates WPF's CommandManager.RequerySuggested: WPF re-evaluates every
// command's CanExecute after input events (mouse clicks, key presses), and
// the shared ViewModels rely on that because they never raise CanExecuteChanged
// themselves. This registers global class handlers that trigger the compat
// RelayCommand's requery after any pointer release or key up, posted at
// background priority so bindings updated by the same input are applied
// first.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace OpenKh.Tools.Common.Avalonia
{
    public static class CommandManagerBehavior
    {
        private static bool _attached;
        private static bool _requeryQueued;

        /// <summary>
        /// Call once at application startup (before the first window opens).
        /// </summary>
        public static void Attach()
        {
            if (_attached)
                return;
            _attached = true;

            InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>(
                (_, _) => QueueRequery(),
                RoutingStrategies.Tunnel,
                handledEventsToo: true);
            InputElement.KeyUpEvent.AddClassHandler<TopLevel>(
                (_, _) => QueueRequery(),
                RoutingStrategies.Tunnel,
                handledEventsToo: true);
        }

        private static void QueueRequery()
        {
            if (_requeryQueued)
                return;
            _requeryQueued = true;
            Dispatcher.UIThread.Post(() =>
            {
                _requeryQueued = false;
                Xe.Tools.Wpf.Commands.RelayCommand.InvalidateRequerySuggested();
            }, DispatcherPriority.Background);
        }
    }
}
