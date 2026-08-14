using OpenKh.Tools.ModsManager.Core.Services;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class GameExtractionService(ModManagerConfigurationService configuration)
{
    private readonly GameDataExtractionService _extraction = new();

    public async Task ExtractAsync(
        IReadOnlyCollection<GameInfo> games,
        IProgress<ModOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (games.Count == 0)
            throw new InvalidOperationException("Select at least one game to extract.");

        ConfigurationService.SkipRemastered = false;
        var completedGames = 0;
        foreach (var game in games)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var gameProgress = new Action<float>(value => progress?.Report(new ModOperationProgress(
                $"Extracting {game.DisplayName}",
                (completedGames + value) / games.Count)));

            if (configuration.Current.GameEdition == 1)
                await ExtractPs2Async(game, gameProgress);
            else if (configuration.Current.GameEdition == 2)
                await ExtractPcAsync(game, gameProgress, cancellationToken);
            else
                throw new InvalidOperationException("Game extraction is available for PC releases and PS2 ISOs.");
            completedGames++;
        }

        configuration.Current.GamesToExtract = games.Select(game => game.Id).ToList();
        configuration.Save();
        progress?.Report(new ModOperationProgress("Game extraction completed", 1));
    }

    private Task ExtractPs2Async(GameInfo game, Action<float> progress)
    {
        var iso = game.Id.ToLowerInvariant() switch
        {
            "kh1" => configuration.Current.IsoLocationKh1,
            "kh2" => configuration.Current.IsoLocationKh2,
            "recom" => configuration.Current.IsoLocationRecom,
            _ => throw new InvalidOperationException($"{game.DisplayName} PS2 extraction is not supported.")
        };
        if (string.IsNullOrWhiteSpace(iso) || !File.Exists(iso))
            throw new FileNotFoundException($"Configure the {game.DisplayName} ISO in Setup.", iso);

        return game.Id.ToLowerInvariant() switch
        {
            "kh1" => _extraction.ExtractKh1Ps2EditionAsync(iso, configuration.GameDataDirectory, progress),
            "kh2" => _extraction.ExtractKh2Ps2EditionAsync(iso, configuration.GameDataDirectory, progress),
            "recom" => _extraction.ExtractRecomPs2EditionAsync(iso, configuration.GameDataDirectory, progress),
            _ => Task.CompletedTask
        };
    }

    private Task ExtractPcAsync(GameInfo game, Action<float> progress, CancellationToken cancellationToken)
    {
        var isKh3D = game.Id.Equals("kh3d", StringComparison.OrdinalIgnoreCase);
        var releaseDirectory = isKh3D
            ? configuration.Current.PcReleaseLocationKh3D
            : configuration.Current.PcReleaseLocation;
        if (string.IsNullOrWhiteSpace(releaseDirectory) || !Directory.Exists(releaseDirectory))
            throw new DirectoryNotFoundException($"Configure the {game.DisplayName} PC release folder in Setup.");

        var languageDirectory = configuration.Current.PcVersion.Equals("Steam", StringComparison.OrdinalIgnoreCase)
            ? "dt"
            : configuration.Current.PcReleaseLanguage.Equals("jp", StringComparison.OrdinalIgnoreCase) ? "jp" : "en";
        return _extraction.ExtractKhPcEditionAsync(
            configuration.GameDataDirectory,
            progress,
            fileName => Path.Combine(configuration.Current.PcReleaseLocation!, "Image", languageDirectory, fileName),
            fileName => Path.Combine(configuration.Current.PcReleaseLocationKh3D!, "Image", languageDirectory, fileName),
            game.Id.Equals("kh1", StringComparison.OrdinalIgnoreCase),
            game.Id.Equals("kh2", StringComparison.OrdinalIgnoreCase),
            game.Id.Equals("bbs", StringComparison.OrdinalIgnoreCase),
            game.Id.Equals("Recom", StringComparison.OrdinalIgnoreCase),
            game.Id.Equals("kh3d", StringComparison.OrdinalIgnoreCase),
            _ => Task.FromResult(false),
            cancellationToken);
    }
}
