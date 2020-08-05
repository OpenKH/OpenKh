using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.IO;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class FileViewModel : EntryViewModel
    {
        private readonly IIdxManager _idxManager;

        public Idx.Entry Entry { get; }
        public string FullName => Entry.GetFullName();

        internal FileViewModel(EntryParserModel entry, IIdxManager idxManager) :
            base(entry.Name)
        {
            _idxManager = idxManager;
            Entry = entry.Entry;

            ExportCommand = new RelayCommand(_ =>
            {
                FileDialog.OnSave(fileName =>
                {
                    File.Create(fileName).Using(stream =>
                        _idxManager.OpenFileFromIdx(Entry).CopyTo(stream));
                }, FileDialogFilterComposer.Compose().AddAllFiles(), Name);
            });
        }

        public RelayCommand ExportCommand { get; }
        public RelayCommand InjectCommand { get; }
        public RelayCommand AppendCommand { get; }
        public RelayCommand ImportCommand { get; }
    }
}
