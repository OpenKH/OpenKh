using System.IO;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class ItemViewModel : ISystemGetChanges
    {
        public string EntryName => "item";

        public Stream CreateStream()
        {
            throw new System.NotImplementedException();
        }
    }
}
