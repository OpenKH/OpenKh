using OpenKh.Kh2;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class FileViewModel : EntryViewModel
    {
        public Idx.Entry Entry { get; }

        internal FileViewModel(EntryParserModel entry) :
            base(entry.Name)
        {
            Entry = entry.Entry;
        }
    }
}
