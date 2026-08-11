using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class InfoWindow : EmbeddedDialogControl
{
    public InfoWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Version {Assembly.GetExecutingAssembly().GetName().Version}";
    }

    private void OpenLink_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is Button { Tag: string url })
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private void Close_OnClick(object? sender, RoutedEventArgs eventArgs) => Close();

    public void HandleControllerAction(ControllerAction action)
    {
        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close();
        else if (action == ControllerAction.Confirm && FocusManager?.GetFocusedElement() is Button button)
        {
            if (button == CloseButton)
                Close();
            else
                OpenLink_OnClick(button, new RoutedEventArgs());
        }
    }
}
