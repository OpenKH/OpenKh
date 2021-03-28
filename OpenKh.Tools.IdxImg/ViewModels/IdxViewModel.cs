using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                FileDialog.OnFolder(x =>
                    Task.Run(() =>
                        ExtractProcessor.ShowProgress(progress =>
                            Extract(x, progress)))));
        }

        public string ShortName => Path.GetFileNameWithoutExtension(Name);
        public RelayCommand ExportCommand { get; }

        private static IEnumerable<EntryViewModel> GetChildren(string idxName, IIdxManager idxManager)
        {
            var idxStream = idxManager.OpenFileFromIdx(idxName);
            if (idxStream == null)
                return new EntryViewModel[0];

            using (idxStream)
            {
                return EntryParserModel.GetChildren(Idx.Read(idxStream), idxManager);
            }
        }

        public override void Extract(string outputPath, IExtractProgress progress)
        {
            foreach (var child in Children)
            {
                if (progress.CancellationToken.IsCancellationRequested)
                    break;

                child.Extract(Path.Combine(outputPath, ShortName), progress);
            }
        }

        public void ExtractAndMerge(string outputPath, IExtractProgress progress)
        {
            foreach (var child in Children)
            {
                if (progress.CancellationToken.IsCancellationRequested)
                    break;

                child.Extract(outputPath, progress);
            }
        }
    }
}
