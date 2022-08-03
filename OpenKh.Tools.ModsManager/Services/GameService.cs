using OpenKh.Common;
using OpenKh.Tools.ModsManager.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class GameService
    {
        private const int BlockIso = 0x800;
        private static readonly List<GameInfoModel> Games = new()
        {
            new GameInfoModel()
            {
                Id = "kh1",
                Name = "Kingdom Hearts I",
                UniqueFileName = "btltbl.bin",
                Detectors = new()
                {
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLPS_251.97;1" },
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLPS_251.98;1" },
                }
            },
            new GameInfoModel()
            {
                Id = "kh2",
                Name = "Kingdom Hearts II",
                UniqueFileName = "00objentry.bin",
                Detectors = new()
                {
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLPM_662.33;1" },
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLUS_210.05;1" },
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLES_541.14;1" },
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLES_542.32;1" },
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLES_542.33;1" },
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLES_542.34;1" },
                    new GameDetectorModel { FileName = "SYSTEM.CNF;1", ProductId = "SLPM_666.75;1" },
                }
            },
        };

        public static GameInfoModel DetectGameId(string isoFilePath)
        {
            using var stream = File.OpenRead(isoFilePath);
            foreach (var game in Games)
            {
                foreach (var detector in game.Detectors)
                {
                    var blockIndex = IsoUtility.GetFileOffset(stream, detector.FileName);
                    if (blockIndex < 0)
                        continue;

                    var data = stream.SetPosition(blockIndex * BlockIso).ReadBytes(BlockIso);
                    var expectData = Encoding.UTF8.GetBytes(detector.ProductId);
                    if (string.Join(' ', data).Contains(string.Join(' ', expectData))) // TODO: inefficient, but it works lol
                        return game; 
                }
            }

            return default;
        }

        public static GameInfoModel Lookup(string gameId) => Games.FirstOrDefault(x => x.Id == gameId);

        public static bool FolderContainsUniqueFile(string gameId, string path)
        {
            var game = Lookup(gameId);
            if (game == null)
                return false;

            return File.Exists(Path.Combine(path, game.UniqueFileName));
        }
    }
}
