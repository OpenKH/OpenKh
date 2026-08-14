namespace OpenKh.Tools.ModsManager.Core;

public static class GameDataDetectionService
{
    public static IReadOnlyList<GameInfo> FindExtractedGames(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory))
            return [];

        return GameInfo.SupportedGames
            .Where(game => IsExtracted(rootDirectory, game))
            .ToArray();
    }

    private static bool IsExtracted(string rootDirectory, GameInfo game)
    {
        var gameDirectory = Path.Combine(rootDirectory, game.Id);
        return game.Id.ToLowerInvariant() switch
        {
            "kh1" => File.Exists(Path.Combine(gameDirectory, "btltbl.bin")),
            "kh2" => File.Exists(Path.Combine(gameDirectory, "00objentry.bin")),
            "bbs" => Directory.Exists(Path.Combine(gameDirectory, "message")),
            "recom" => File.Exists(Path.Combine(gameDirectory, "CST_sora.pss")) ||
                       Directory.Exists(Path.Combine(gameDirectory, "SYS")),
            "kh3d" => Directory.Exists(Path.Combine(gameDirectory, "setdata")),
            _ => false
        };
    }
}
