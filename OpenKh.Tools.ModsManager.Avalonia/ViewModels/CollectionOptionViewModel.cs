using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.ViewModels;

public sealed class CollectionOptionViewModel(CollectionOption option) : ObservableObject
{
    private bool _isEnabled = option.IsEnabled;

    public string Name => option.Name;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public CollectionOption ToModel() => new(Name, IsEnabled);
}
