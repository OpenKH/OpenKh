using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            ExportCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(x =>
                    Task.Run(() =>
                        ExtractProcessor.ShowProgress(progress =>
                            Extract(x, progress)))));
            ExportAndMergeCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(x =>
                    Task.Run(() =>
                        ExtractProcessor.ShowProgress(progress =>
                            ExtractAndMerge(x, progress)))));
        }

        public string ShortName => Path.GetFileNameWithoutExtension(Name);
        public RelayCommand ExportCommand { get; }
        public RelayCommand ExportAndMergeCommand { get; }

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

                var childOutputPath = Path.Combine(outputPath, ShortName);
                if (child is IdxViewModel idxVm)
                    idxVm.ExtractAndMerge(childOutputPath, progress);
                else
                    child.Extract(childOutputPath, progress);
            }
        }
    }
}
