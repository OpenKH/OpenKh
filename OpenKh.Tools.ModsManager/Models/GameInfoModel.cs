using System.Collections.Generic;

namespace OpenKh.Tools.ModsManager.Models
{
    public record GameInfoModel
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string UniqueFileName { get; init; }
        public List<GameDetectorModel> Detectors { get; init; }
    }

    public record GameDetectorModel
    {
        public string FileName { get; init; }
        public string ProductId { get; init; }
    }
}
