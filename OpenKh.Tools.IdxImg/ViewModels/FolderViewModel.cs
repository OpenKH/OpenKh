using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class FolderViewModel : NodeViewModel
    {
        internal FolderViewModel(string name, int depth, IEnumerable<EntryParserModel> entries) :
            base(name, EntryParserModel.GetEntries(entries.ToList(), depth))
        {
        }
    }
}
