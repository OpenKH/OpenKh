using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface ICollectionSettingsPrompt
{
    Task<bool> ShowAsync(ModEntry mod, GameInfo game);
}
