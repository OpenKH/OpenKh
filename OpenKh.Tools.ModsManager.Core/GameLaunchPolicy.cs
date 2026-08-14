namespace OpenKh.Tools.ModsManager.Core;

public static class GameLaunchPolicy
{
    public static bool ShouldUseSteamClient(
        string? pcVersion,
        bool directLaunchConfigured,
        bool isLinux) =>
        string.Equals(pcVersion, "Steam", StringComparison.OrdinalIgnoreCase) &&
        (isLinux || !directLaunchConfigured);
}
