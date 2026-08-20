using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.Common.Avalonia.Internal
{
    public static class WindowLocator
    {
        public static Window GetActiveWindow()
        {
            if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (var window in desktop.Windows)
                    if (window.IsActive)
                        return window;
                return desktop.MainWindow;
            }
            return null;
        }

        /// <summary>
        /// Blocks the calling UI thread on an async operation by pumping a
        /// nested dispatcher frame, the same way WPF's ShowDialog blocks.
        /// Must be called from the UI thread.
        /// </summary>
        public static T WaitOnUIThread<T>(Task<T> task)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false));
            Dispatcher.UIThread.PushFrame(frame);
            return task.GetAwaiter().GetResult();
        }
    }
}
