using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.Launcher;

internal sealed class MessageDialog : Window
{
    private IDisposable? _controllerCapture;

    public MessageDialog(string title, string message, bool showCancel = false)
    {
        Title = title;
        Width = 520;
        MinWidth = 360;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#0D1420");

        var okButton = new Button
        {
            Content = showCancel ? "Continue" : "OK",
            Classes = { "primary" },
            MinWidth = 110,
        };
        okButton.Click += (_, _) => Close(true);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children = { okButton },
        };

        if (showCancel)
        {
            var cancelButton = new Button
            {
                Content = "Cancel",
                Classes = { "secondary" },
                MinWidth = 110,
            };
            cancelButton.Click += (_, _) => Close(false);
            buttonPanel.Children.Insert(0, cancelButton);
        }

        Content = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 22,
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    Foreground = Brush.Parse("#F5F7FB"),
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 22,
                },
                buttonPanel,
            },
        };

        Opened += (_, _) =>
        {
            okButton.Focus();
            if (Avalonia.Application.Current is App app)
                _controllerCapture = app.Controller.Capture(HandleControllerAction);
        };
        Closed += (_, _) => _controllerCapture?.Dispose();
    }

    public static async Task<bool> ShowAsync(Window owner, string title, string message, bool showCancel = false) =>
        await new MessageDialog(title, message, showCancel).ShowDialog<bool>(owner);

    private void HandleControllerAction(ControllerAction action)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ControllerWindowNavigator.TryMoveFocus(this, action))
                return;

            if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem or ControllerAction.PreviousGame)
            {
                ControllerWindowNavigator.MoveFocus(this, -1);
                return;
            }
            if (action is ControllerAction.NextControl or ControllerAction.NextItem or ControllerAction.NextGame)
            {
                ControllerWindowNavigator.MoveFocus(this, 1);
                return;
            }
            if (action == ControllerAction.Cancel)
            {
                Close(false);
                return;
            }
            if (action == ControllerAction.Confirm && FocusManager?.GetFocusedElement() is Button button)
            {
                button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                return;
            }
            if (action == ControllerAction.Confirm)
                Close(true);
        });
    }
}
