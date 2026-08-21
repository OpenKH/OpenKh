using OpenKh.Tools.ModsManager.Core;
using Avalonia.Media.Imaging;

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
        IconImage = LoadImage(model.IconPath);
        PreviewImage = LoadImage(model.PreviewPath);
    }

    public ModEntry Model { get; }
    public string Id => Model.Id;
    public string Name => Model.Name;
    public string Author => Model.Author;
    public string Description => Model.Description;
    public string Directory => Model.Directory;
    public string? SourceUrl => Model.SourceUrl;
    public string? ReportBugUrl => Model.ReportBugUrl;
    public bool HasSource => !string.IsNullOrWhiteSpace(SourceUrl);
    public bool CanReportBug => !string.IsNullOrWhiteSpace(ReportBugUrl);
    public string Kind => Model.IsCollection ? "COLLECTION" : "MOD";
    public bool IsCollection => Model.IsCollection;
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name[..1].ToUpperInvariant();
    public Bitmap? IconImage { get; }
    public Bitmap? PreviewImage { get; }
    public IReadOnlyList<string> FilesToPatch => Model.FilesToPatch;
    public bool HasFilesToPatch => FilesToPatch.Count > 0;
    public string FilesToPatchHeader => FilesToPatch.Count == 1
        ? "1 file modified"
        : $"{FilesToPatch.Count} files modified";
    public string FilesToPatchText => string.Join(Environment.NewLine, FilesToPatch);
    public bool HasIcon => IconImage is not null;
    public bool HasPreview => PreviewImage is not null;
    public bool ShowInitial => !HasIcon;
    public int UpdateCount => Model.UpdateCount;
    public bool HasUpdate => UpdateCount > 0;
    public string UpdateLabel => UpdateCount == 1 ? "1 UPDATE" : $"{UpdateCount} UPDATES";

    public void SetUpdateCount(int count)
    {
        Model.UpdateCount = Math.Max(0, count);
        OnPropertyChanged(nameof(UpdateCount));
        OnPropertyChanged(nameof(HasUpdate));
        OnPropertyChanged(nameof(UpdateLabel));
    }

    private static Bitmap? LoadImage(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            return null;
        try
        {
            return new Bitmap(fileName);
        }
        catch
        {
            return null;
        }
    }

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
