namespace OpenKh.Tools.ModsManager.Core;

public sealed record GameInfo(string Id, string DisplayName, string EnabledModsFileName, string ConfigFileSuffix)
{
    public static IReadOnlyList<GameInfo> SupportedGames { get; } =
    [
        new("kh2", "Kingdom Hearts II", "mods-KH2.txt", "KH2"),
        new("kh1", "Kingdom Hearts", "mods-KH1.txt", "KH1"),
        new("bbs", "Birth by Sleep", "mods-BBS.txt", "BBS"),
        new("Recom", "Re:Chain of Memories", "mods-ReCoM.txt", "ReCoM"),
        new("kh3d", "Dream Drop Distance", "mods-KH3D.txt", "KH3D")
    ];

    public static GameInfo FromId(string? id) =>
        SupportedGames.FirstOrDefault(game =>
            game.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ?? SupportedGames[0];
}
