using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenKh.Tools.BarTool.Dialogs
{
    class MessageBox : Window
    {
        public enum MessageBoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel,
            None
        }

        public enum MessageBoxResult
        {
            Ok,
            Cancel,
            Yes,
            No
        }

        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<MessageBoxResult> Show(Window Parent, string Message, string Title, MessageBoxButtons Buttons)
        {
            var _box = new MessageBox()
            {
                Title = Title
            };

            _box.FindControl<TextBlock>("Text").Text = Message;
            var _buttonPanel = _box.FindControl<StackPanel>("Buttons");


            var _result = MessageBoxResult.Ok;

            void AddButton(string Caption, MessageBoxResult Result, bool Def = false)
            {
                var _buttonPan_tempButtonel = new Button { Content = Caption };

                _buttonPan_tempButtonel.Click += (_, __) => 
                {
                    _result = Result;
                    _box.Close();
                };

                _buttonPanel.Children.Add(_buttonPan_tempButtonel);

                if (Def)
                    _result = Result;
            }

            if (Buttons == MessageBoxButtons.Ok || Buttons == MessageBoxButtons.OkCancel)
                AddButton("Ok", MessageBoxResult.Ok, true);
            if (Buttons == MessageBoxButtons.YesNo || Buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, true);
            }

            if (Buttons == MessageBoxButtons.OkCancel || Buttons == MessageBoxButtons.YesNoCancel)
                AddButton("Cancel", MessageBoxResult.Cancel, true);


            var _source = new TaskCompletionSource<MessageBoxResult>();
            _box.Closed += delegate { _source.TrySetResult(_result); };
            
            if (Parent != null)
                _box.ShowDialog(Parent);
            
            else
                _box.Show();
            
            return _source.Task;
        }
    }
}
