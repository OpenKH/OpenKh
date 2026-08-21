using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class ConfirmationWindow : EmbeddedDialogControl
{
    public ConfirmationWindow()
        : this("Confirm action", string.Empty, "Confirm")
    {
    }

    public ConfirmationWindow(string title, string message, string confirmText)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
        ConfirmButton.Content = confirmText;
        Opened += (_, _) => ConfirmButton.Focus();
    }

    private void Confirm_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(true);
    private void Cancel_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(false);

    public void HandleControllerAction(ControllerAction action)
    {
        if (ControllerWindowNavigator.TryMoveFocus(this, action))
            return;
        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem or ControllerAction.PreviousGame)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem or ControllerAction.NextGame)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close(false);
        else if (action == ControllerAction.Confirm)
            Close(FocusManager?.GetFocusedElement() == ConfirmButton);
    }
}
