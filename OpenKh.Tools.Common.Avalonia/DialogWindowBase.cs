using Avalonia.Controls;
using OpenKh.Tools.Common.Avalonia.Internal;

namespace OpenKh.Tools.Common.Avalonia
{
    /// <summary>
    /// Window base class exposing WPF-style dialog semantics (a synchronous,
    /// parameterless ShowDialog returning bool? and a settable DialogResult)
    /// so shared ViewModels can drive Avalonia windows unchanged.
    /// </summary>
    public class DialogWindowBase : Window
    {
        public bool? DialogResult { get; set; }

        /// <summary>
        /// WPF-compat: an assignable Owner. Avalonia only sets its own Owner
        /// through Show(owner)/ShowDialog(owner), so this shadows it with a
        /// settable property that Show() honors.
        /// </summary>
        public new Window Owner { get; set; }

        /// <summary>
        /// WPF-compat: accepts the System.Windows.WindowStartupLocation enum
        /// used by the shared ViewModels and forwards to Avalonia's.
        /// </summary>
        public new System.Windows.WindowStartupLocation WindowStartupLocation
        {
            get => (System.Windows.WindowStartupLocation)(int)base.WindowStartupLocation;
            set => base.WindowStartupLocation = (global::Avalonia.Controls.WindowStartupLocation)(int)value;
        }

        public new void Show()
        {
            if (Owner is not null)
                Show(Owner);
            else
                base.Show();
        }

        public bool? ShowDialog()
        {
            var owner = WindowLocator.GetActiveWindow();
            if (owner is not null && owner != this)
                return WindowLocator.WaitOnUIThread(ShowDialog<bool?>(owner)) ?? DialogResult;

            // No owner yet (e.g. first-run wizard before the main window):
            // show standalone and pump until closed.
            var frame = new global::Avalonia.Threading.DispatcherFrame();
            Closed += (_, _) => frame.Continue = false;
            Show();
            global::Avalonia.Threading.Dispatcher.UIThread.PushFrame(frame);
            return DialogResult;
        }

        protected void CloseWithResult(bool? result)
        {
            DialogResult = result;
            Close(result);
        }
    }
}
