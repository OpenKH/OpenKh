namespace OpenKh.Tools.ModsManager.Core;

public sealed class SteamAppIdService
{
    private const string FileName = "steam_appid.txt";

    public bool IsInstalled(string? releaseDirectory, bool isKh3D)
    {
        var fileName = GetFileName(releaseDirectory, false);
        return fileName is not null &&
               File.Exists(fileName) &&
               File.ReadAllText(fileName).Trim().Equals(GetAppId(isKh3D), StringComparison.Ordinal);
    }

    public void Install(string? releaseDirectory, bool isKh3D)
    {
        var fileName = GetFileName(releaseDirectory, true)!;
        File.WriteAllText(fileName, GetAppId(isKh3D));
    }

    public void Remove(string? releaseDirectory, bool isKh3D)
    {
        var fileName = GetFileName(releaseDirectory, true)!;
        if (!File.Exists(fileName))
            return;
        if (!File.ReadAllText(fileName).Trim().Equals(GetAppId(isKh3D), StringComparison.Ordinal))
            throw new InvalidOperationException("The existing steam_appid.txt was not created for this game and will not be removed.");
        File.Delete(fileName);
    }

    private static string GetAppId(bool isKh3D) => isKh3D ? "2552440" : "2552430";

    private static string? GetFileName(string? releaseDirectory, bool required)
    {
        if (!string.IsNullOrWhiteSpace(releaseDirectory) && Directory.Exists(releaseDirectory))
            return Path.Combine(releaseDirectory, FileName);
        if (required)
            throw new DirectoryNotFoundException("Configure a valid Steam game folder first.");
        return null;
    }
}
