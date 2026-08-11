namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IPresetsPrompt
{
    Task<IReadOnlyList<string>?> ShowAsync(IReadOnlyCollection<string> enabledModIds);
}
