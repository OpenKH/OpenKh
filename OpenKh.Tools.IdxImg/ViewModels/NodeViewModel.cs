using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public abstract class NodeViewModel : EntryViewModel
    {
        public ObservableCollection<EntryViewModel> Children { get; }

        public NodeViewModel(string name, IEnumerable<EntryViewModel> entries) :
            base(name)
        {
            Children = new ObservableCollection<EntryViewModel>(entries);
        }
    }
}
