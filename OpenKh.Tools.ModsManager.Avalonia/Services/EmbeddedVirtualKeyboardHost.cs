using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using OpenKh.Tools.ModsManager.Avalonia.Views;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public sealed class EmbeddedVirtualKeyboardHost(MainWindow owner) : IVirtualKeyboardHost
{
    private ControllerKeyboardWindow? _keyboard;

    public bool IsOpen => _keyboard is not null;

    public void Show(TextBox textBox)
    {
        if (_keyboard is not null)
            return;

        textBox.Focus(NavigationMethod.Directional);
        var keyboard = new ControllerKeyboardWindow(textBox);
        _keyboard = keyboard;
        _ = ShowAsync(keyboard, textBox);
    }

    public bool HandleControllerAction(ControllerAction action)
    {
        if (_keyboard is null)
            return false;

        _keyboard.HandleControllerAction(action);
        return true;
    }

    public bool Hide()
    {
        if (_keyboard is null)
            return false;

        _keyboard.Close();
        return true;
    }

    private async Task ShowAsync(ControllerKeyboardWindow keyboard, TextBox textBox)
    {
        try
        {
            await owner.ShowPageAsync<object?>(keyboard);
        }
        finally
        {
            if (ReferenceEquals(_keyboard, keyboard))
                _keyboard = null;

            Dispatcher.UIThread.Post(
                () => textBox.Focus(NavigationMethod.Directional),
                DispatcherPriority.Loaded);
        }
    }
}
