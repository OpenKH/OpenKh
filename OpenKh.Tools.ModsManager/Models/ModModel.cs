using OpenKh.Patcher;

namespace OpenKh.Tools.ModsManager.Models
{
    public class ModModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Metadata Metadata { get; set; }
        public bool IsEnabled { get; set; }
    }
}
