using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class IdxViewModel : NodeViewModel
    {
        private readonly IIdxManager _idxManager;

        internal IdxViewModel(string name, Idx.Entry entry, IIdxManager idxManager) :
            base(name, GetChildren(name, idxManager))
        {
            _idxManager = idxManager;
            ExportCommand = new RelayCommand(_ =>
            {
                FileDialog.OnFolder(Extract, Name);
            });
        }

        public RelayCommand ExportCommand { get; }

        private static IEnumerable<EntryViewModel> GetChildren(string idxName, IIdxManager idxManager)
        {
            var idxStream = idxManager.OpenFileFromIdx(idxName);
            if (idxStream == null)
                return new EntryViewModel[0];

            using (idxStream)
            {
                var myEntries = Idx.Read(idxStream)
                    .Select(x => new EntryParserModel(x))
                    .OrderBy(x => x.Path)
                    .ToList();

                return EntryParserModel.GetEntries(myEntries, 0, idxManager);
            }
        }

        public override void Extract(string outputPath)
        {
            foreach (var child in Children)
                child.Extract(Path.Combine(outputPath, Name));
        }
    }
}
