using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

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
            ExportCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(x =>
                    Task.Run(() =>
                        ExtractProcessor.ShowProgress(progress =>
                            Extract(x, progress)))));
        }

        public RelayCommand ExportCommand { get; }

        public override void Extract(string outputPath, IExtractProgress progress)
        {
            var childOutputPath = Path.Combine(outputPath, Name);
            Directory.CreateDirectory(childOutputPath);

            foreach (var child in Children)
            {
                if (progress.CancellationToken.IsCancellationRequested)
                    break;

                progress.SetExtractedName(childOutputPath);
                child.Extract(childOutputPath, progress);
            }
        }
    }
}
