using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IOnlineModsPrompt
{
    Task<bool> ShowAsync(
        GameInfo game,
        IReadOnlyCollection<string> installedIds,
        Func<Task> onModInstalled);
}
