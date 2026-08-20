// WPF compatibility shim: maps `Application.Current.Dispatcher.Invoke(...)`
// call sites in shared sources onto Avalonia's Dispatcher.UIThread.
// See WpfEnums.cs for why these live in the System.Windows namespace.

namespace System.Windows
{
    public sealed class Application
    {
        public static Application Current { get; } = new Application();

        private Application()
        {
        }

        public Dispatcher Dispatcher { get; } = new Dispatcher();
    }

    public sealed class Dispatcher
    {
        public void Invoke(Action action)
        {
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                action();
            else
                Avalonia.Threading.Dispatcher.UIThread.Invoke(action);
        }

        public T Invoke<T>(Func<T> func)
        {
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                return func();
            return Avalonia.Threading.Dispatcher.UIThread.Invoke(func);
        }

        public DispatcherOperation InvokeAsync(Action action) =>
            new DispatcherOperation(Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(action).GetTask());
    }

    /// <summary>
    /// Mirrors WPF's DispatcherOperation just enough for the shared sources:
    /// awaitable directly and exposing a Task property.
    /// </summary>
    public sealed class DispatcherOperation
    {
        internal DispatcherOperation(System.Threading.Tasks.Task task) => Task = task;

        public System.Threading.Tasks.Task Task { get; }

        public System.Runtime.CompilerServices.TaskAwaiter GetAwaiter() => Task.GetAwaiter();
    }
}
