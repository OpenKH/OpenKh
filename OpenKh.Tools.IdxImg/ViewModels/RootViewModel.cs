using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    class RootViewModel : NodeViewModel
    {
        private readonly IIdxManager _idxManager;

        public RootViewModel(string name, List<Idx.Entry> entries, IIdxManager idxManager) :
            base(name, EntryParserModel.GetChildren(entries, idxManager))
        {
            _idxManager = idxManager;
            ExportCommand = new RelayCommand(_ => FileDialog.OnFolder(Extract));
        }

        public string ShortName => Path.GetFileNameWithoutExtension(Name);
        public RelayCommand ExportCommand { get; }

        public override void Extract(string outputPath)
        {
            foreach (var child in Children)
                child.Extract(Path.Combine(outputPath, ShortName));
        }
    }
}
