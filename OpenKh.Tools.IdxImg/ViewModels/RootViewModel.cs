using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    class RootViewModel
    {
        private readonly IIdxManager _idxManager;

        public RootViewModel(string name, IEnumerable<Idx.Entry> entries, IIdxManager idxManager)
        {
            var myEntries = entries
                .Select(x => new EntryParserModel(x))
                .OrderBy(x => x.Path)
                .ToList();

            _idxManager = idxManager;
            Name = name;
            Children = new ObservableCollection<EntryViewModel>(
                EntryParserModel.GetEntries(myEntries, 0, idxManager));
        }

        public string Name { get; }

        public ObservableCollection<EntryViewModel> Children { get; }

        public RelayCommand ExportCommand { get; }
    }
}
