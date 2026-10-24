using System;

namespace OpenKh.Tools.ModManager.Models
{
    public class GitModel
    {
        public required string Author { get; set; }
        public required string Repository { get; set; }
        public required string Platform { get; set; }
        public string? Branch { get; set; }

        public DateTime? LatestCommit { get; set; }
    }
}
