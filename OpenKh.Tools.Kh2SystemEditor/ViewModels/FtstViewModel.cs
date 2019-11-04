using System.IO;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class FtstViewModel : ISystemGetChanges
    {
        public string EntryName => "ftst";

        public Stream CreateStream()
        {
            throw new System.NotImplementedException();
        }
    }
}
