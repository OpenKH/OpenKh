using OpenKh.Kh2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class RootViewModel
    {
        public RootViewModel(string name, IEnumerable<Idx.Entry> entries)
        {
            var myEntries = entries
                .Select(x => new EntryParserModel(x))
                .OrderBy(x => x.Path)
                .ToList();

            Name = name;
            Children = new ObservableCollection<EntryViewModel>(
                EntryParserModel.GetEntries(myEntries, 0));
        }

        public string Name { get; }

        public ObservableCollection<EntryViewModel> Children { get; }
    }
}
