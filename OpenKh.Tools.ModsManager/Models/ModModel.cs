using OpenKh.Patcher;
using System.Collections.Generic;

namespace OpenKh.Tools.ModsManager.Models
{
    public class ModModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string IconImageSource { get; set; }
        public string PreviewImageSource { get; set; }
        public Metadata Metadata { get; set; }
        public bool IsEnabled { get; set; }
        public Dictionary<string, bool> CollectionOptionalEnabledAssets { get; set; }
    }
}
