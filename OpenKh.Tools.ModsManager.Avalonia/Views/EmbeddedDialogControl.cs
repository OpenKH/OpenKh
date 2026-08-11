using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public abstract class EmbeddedDialogControl : UserControl
{
    private readonly TaskCompletionSource<object?> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _closed;

    public event EventHandler? Opened;
    public event EventHandler? Closed;

    protected IStorageProvider StorageProvider =>
        TopLevel.GetTopLevel(this)?.StorageProvider ??
        throw new InvalidOperationException("The page is not attached to a window.");

    protected IFocusManager? FocusManager => TopLevel.GetTopLevel(this)?.FocusManager;

    internal Task<object?> ShowEmbeddedAsync()
    {
        Opened?.Invoke(this, EventArgs.Empty);
        return _completion.Task;
    }

    public void Close() => Close(null);

    public void Close(object? result)
    {
        if (_closed)
            return;

        _closed = true;
        Closed?.Invoke(this, EventArgs.Empty);
        _completion.TrySetResult(result);
    }
}
