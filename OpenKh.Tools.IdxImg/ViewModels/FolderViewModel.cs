using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class FolderViewModel : NodeViewModel
    {
        private readonly IIdxManager _idxManager;

        internal FolderViewModel(
            string name, int depth, IEnumerable<EntryParserModel> entries, IIdxManager idxManager) :
            base(name, EntryParserModel.GetEntries(entries.ToList(), depth, idxManager))
        {
            _idxManager = idxManager;
        }
    }
}
