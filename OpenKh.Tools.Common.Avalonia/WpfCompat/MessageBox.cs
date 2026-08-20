// WPF compatibility shim: a synchronous MessageBox.Show(...) with the same
// overloads and result semantics as WPF's, rendered as an Avalonia dialog.
// Synchronous blocking is implemented by pumping a nested DispatcherFrame
// until the dialog closes, mirroring how WPF's ShowDialog works internally.
// See WpfEnums.cs for why this lives in the System.Windows namespace.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace System.Windows
{
    public static class MessageBox
    {
        public static MessageBoxResult Show(string messageBoxText) =>
            Show(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);

        public static MessageBoxResult Show(string messageBoxText, string caption) =>
            Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button) =>
            Show(messageBoxText, caption, button, MessageBoxImage.None);

        public static MessageBoxResult Show(
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxOptions options = MessageBoxOptions.None)
        {
            if (!global::Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                return global::Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                    Show(messageBoxText, caption, button, icon, defaultResult, options));

            var dialog = BuildDialog(messageBoxText, caption, button, icon, defaultResult);
            var owner = GetActiveWindow();

            var result = FallbackResult(button);
            dialog.Window.Closed += (_, _) => result = dialog.Result ?? FallbackResult(button);

            var frame = new DispatcherFrame();
            dialog.Window.Closed += (_, _) => frame.Continue = false;

            if (owner is not null && owner != dialog.Window)
                _ = dialog.Window.ShowDialog(owner);
            else
                dialog.Window.Show();

            global::Avalonia.Threading.Dispatcher.UIThread.PushFrame(frame);
            return result;
        }

        private static MessageBoxResult FallbackResult(MessageBoxButton button) =>
            // Result when the dialog is closed via the title bar / Esc,
            // matching WPF: OK-only boxes return OK, everything else Cancel/No.
            button switch
            {
                MessageBoxButton.OK => MessageBoxResult.OK,
                MessageBoxButton.YesNo => MessageBoxResult.No,
                _ => MessageBoxResult.Cancel,
            };

        private static AvaloniaWindow GetActiveWindow()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.Windows.FirstOrDefault(x => x.IsActive) ?? desktop.MainWindow;
            return null;
        }

        private sealed class DialogHolder
        {
            public AvaloniaWindow Window { get; init; }
            public MessageBoxResult? Result { get; set; }
        }

        private static DialogHolder BuildDialog(
            string text,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult)
        {
            var window = new AvaloniaWindow
            {
                Title = caption ?? string.Empty,
                SizeToContent = SizeToContent.WidthAndHeight,
                CanResize = false,
                WindowStartupLocation = global::Avalonia.Controls.WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false,
                MinWidth = 320,
                MaxWidth = 640,
            };
            var holder = new DialogHolder { Window = window };

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
            };

            void AddButton(string content, MessageBoxResult result, bool isDefault, bool isCancel)
            {
                var btn = new Button
                {
                    Content = content,
                    MinWidth = 88,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    IsDefault = isDefault || result == defaultResult,
                    IsCancel = isCancel,
                };
                btn.Click += (_, _) =>
                {
                    holder.Result = result;
                    window.Close();
                };
                buttonsPanel.Children.Add(btn);
            }

            switch (button)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK, isDefault: true, isCancel: true);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, isDefault: true, isCancel: false);
                    AddButton("Cancel", MessageBoxResult.Cancel, isDefault: false, isCancel: true);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton("Yes", MessageBoxResult.Yes, isDefault: true, isCancel: false);
                    AddButton("No", MessageBoxResult.No, isDefault: false, isCancel: false);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton("Yes", MessageBoxResult.Yes, isDefault: true, isCancel: false);
                    AddButton("No", MessageBoxResult.No, isDefault: false, isCancel: false);
                    AddButton("Cancel", MessageBoxResult.Cancel, isDefault: false, isCancel: true);
                    break;
            }

            var messagePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
            };

            var glyph = icon switch
            {
                MessageBoxImage.Error => "❌",
                MessageBoxImage.Warning => "⚠",
                MessageBoxImage.Question => "❓",
                MessageBoxImage.Information => "ℹ",
                _ => null,
            };
            if (glyph is not null)
                messagePanel.Children.Add(new TextBlock
                {
                    Text = glyph,
                    FontSize = 28,
                    VerticalAlignment = VerticalAlignment.Top,
                });

            messagePanel.Children.Add(new TextBlock
            {
                Text = text ?? string.Empty,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 480,
                VerticalAlignment = VerticalAlignment.Center,
            });

            window.Content = new StackPanel
            {
                Margin = new Thickness(24, 20),
                Spacing = 20,
                Children = { messagePanel, buttonsPanel },
            };

            return holder;
        }
    }
}
