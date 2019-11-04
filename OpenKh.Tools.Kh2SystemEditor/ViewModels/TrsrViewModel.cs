using System.IO;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class TrsrViewModel : ISystemGetChanges
    {
        public string EntryName => "trsr";

        public Stream CreateStream()
        {
            throw new System.NotImplementedException();
        }
    }
}
