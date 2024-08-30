using OpenKh.Tools.ModsManager.Services;
using System.Threading;
using Xunit;

namespace OpenKh.Tests.ModsManager
{
    public class GameDataExtractionServiceTest
    {
        private readonly GameDataExtractionService _gameDataExtractionService = new GameDataExtractionService();

        //[Fact]
        public async Task ExtractKh2Ps2EditionAsyncTest()
        {
            await _gameDataExtractionService.ExtractKh2Ps2EditionAsync(
                isoLocation: @"H:\CCD\KH2fm.ISO",
                gameDataLocation: @"H:\Tmp\ModsManagerExtractionRoot",
                onProgress: (progress) => Console.WriteLine($"{progress:P}")
            );
        }

        //[Fact]
        public async Task ExtractKhPcEditionAsyncTest()
        {
            var pcReleaseLocation = @"H:\Program Files\Epic Games/KH_1.5_2.5";
            var pcReleaseLocationKH3D = @"H:\Program Files\Epic Games/KH_2.8";
            var pcReleaseLanguage = "jp";
            var langFolder = (ConfigurationService.PCVersion == "Steam" && pcReleaseLanguage == "en") ? "dt" : pcReleaseLanguage;

            await _gameDataExtractionService.ExtractKhPcEditionAsync(
                gameDataLocation: @"H:\Tmp\ModsManagerPcExtractionRoot",
                onProgress: (progress) => Console.WriteLine($"{progress:P}"),
                getKhFilePath: fileName
                    => Path.Combine(
                        pcReleaseLocation,
                        "Image",
                        langFolder,
                        fileName
                    ),
                getKh3dFilePath: fileName
                    => Path.Combine(
                        pcReleaseLocationKH3D,
                        "Image",
                        langFolder,
                        fileName
                    ),
                extractkh1: true,
                extractkh2: true,
                extractbbs: true,
                extractrecom: true,
                extractkh3d: true,
                ifRetry: ex => Task.FromResult(false),
                cancellationToken: CancellationToken.None
            );
        }
    }
}
