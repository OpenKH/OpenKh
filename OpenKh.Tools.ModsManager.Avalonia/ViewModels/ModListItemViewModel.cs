using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.ViewModels;

public sealed class ModListItemViewModel : ObservableObject
{
    private readonly Action _enabledStateChanged;
    private bool _isEnabled;

    public ModListItemViewModel(ModEntry model, Action enabledStateChanged)
    {
        Model = model;
        _isEnabled = model.IsEnabled;
        _enabledStateChanged = enabledStateChanged;
    }

    public ModEntry Model { get; }
    public string Id => Model.Id;
    public string Name => Model.Name;
    public string Author => Model.Author;
    public string Description => Model.Description;
    public string Directory => Model.Directory;
    public string Kind => Model.IsCollection ? "COLLECTION" : "MOD";
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name[..1].ToUpperInvariant();

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (!SetProperty(ref _isEnabled, value))
                return;

            Model.IsEnabled = value;
            _enabledStateChanged();
        }
    }
}
