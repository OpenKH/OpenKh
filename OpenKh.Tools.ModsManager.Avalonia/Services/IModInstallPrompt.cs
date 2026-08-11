namespace OpenKh.Tools.ModsManager.Avalonia.Services;

public interface IModInstallPrompt
{
    Task<ModInstallRequest?> ShowAsync();
}

public sealed record ModInstallRequest(string Source, string? Branch, bool Overwrite);
