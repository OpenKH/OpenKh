namespace OpenKh.Tools.ModsManager.Core;

internal static class PanaceaPath
{
    internal static string ToLoaderPath(string path, bool? isLinux = null)
    {
        if (!(isLinux ?? OperatingSystem.IsLinux()) || !path.StartsWith('/'))
            return path;

        // Panacea runs as Windows code inside Proton, where the Z: drive exposes the Linux filesystem.
        return $"Z:{path.Replace('/', '\\')}";
    }
}
