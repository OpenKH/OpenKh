using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class FileViewModel : EntryViewModel
    {
        private readonly IIdxManager _idxManager;

        public Idx.Entry Entry { get; }

        internal FileViewModel(EntryParserModel entry, IIdxManager idxManager) :
            base(entry.Name)
        {
            _idxManager = idxManager;
            Entry = entry.Entry;
        }
    }
}
