using OpenKh.Tools.ModsManager.Interfaces;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class Pcsx2OperationDispatcher(
    ModManagerConfigurationService configuration,
    GameInfo game) : IOperationDispatcher
{
    private static readonly string[] RegionFallback = ["us", "fm", "jp", "uk", "it", "fr", "es", "de"];
    private static readonly HashSet<string> DenyList = new(StringComparer.OrdinalIgnoreCase)
    {
        "dkmovie.x", "dktitle.x", "gb.x", "wm.x", "xl_limit.x", "xs_bambi.x", "xs_dh_break.x",
        "xs_dumbo.x", "xs_genie.x", "xs_mushu.x", "xs_simba.x", "xs_tink.x", "ovl_title.x",
        "ovl_shop.x", "ovl_movie.x", "ovl_gumibattle.x", "ovl_gumimenu.x"
    };

    public int LoadFile(Stream outStream, string fileName)
    {
        var finalFileName = ResolveFile(fileName);
        if (finalFileName is null)
            return 0;

        using var input = File.OpenRead(finalFileName);
        input.CopyTo(outStream, 512 * 1024);
        return checked((int)input.Length);
    }

    public int GetFileSize(string fileName)
    {
        var finalFileName = ResolveFile(fileName);
        return finalFileName is null ? 0 : checked((int)new FileInfo(finalFileName).Length);
    }

    private string? ResolveFile(string fileName)
    {
        if (DenyList.Contains(fileName))
            return null;

        var modRoot = configuration.GetGameModOutputDirectory(game);
        var dataRoot = Path.Combine(configuration.GameDataDirectory, game.Id);
        foreach (var candidate in EnumerateCandidates(fileName))
        {
            var resolved = ResolveCandidate(modRoot, candidate) ?? ResolveCandidate(dataRoot, candidate);
            if (resolved is not null)
                return resolved;
        }

        return null;
    }

    private static IEnumerable<string> EnumerateCandidates(string fileName)
    {
        yield return fileName;
        var region = RegionFallback.FirstOrDefault(candidate =>
            fileName.Contains($"/{candidate}/", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith($".a.{candidate}", StringComparison.OrdinalIgnoreCase));
        if (region is null)
            yield break;

        foreach (var fallback in RegionFallback)
        {
            yield return fileName
                .Replace($"/{region}/", $"/{fallback}/", StringComparison.OrdinalIgnoreCase)
                .Replace($".a.{region}", $".a.{fallback}", StringComparison.OrdinalIgnoreCase)
                .Replace(".apdx", $".a.{fallback}", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string? ResolveCandidate(string root, string relativePath)
    {
        var candidate = Path.Combine(root, relativePath);
        if (!File.Exists(candidate))
            return null;

        var file = new FileInfo(candidate);
        return file.LinkTarget is null
            ? file.FullName
            : Path.GetFullPath(file.LinkTarget, file.DirectoryName!);
    }
}
