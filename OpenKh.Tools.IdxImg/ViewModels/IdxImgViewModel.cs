using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using OpenKh.Tools.IdxImg.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using forms = System.Windows.Forms;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using OpenKh.Tools.Common;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    class IdxImgViewModel : BaseNotifyPropertyChanged, IIdxManager, ITreeSelectedItem
    {
        private static readonly IEnumerable<FileDialogFilter> Filter =
            FileDialogFilterComposer.Compose()
            .AddExtensions("KH2.IDX or KH2.IMG from the ISO or game disc", "IDX", "IMG")
            .AddAllFiles();
        private static readonly IEnumerable<FileDialogFilter> IdxFilter =
            FileDialogFilterComposer.Compose()
            .AddExtensions("KH2.IDX from the ISO or game disc", "IDX")
            .AddAllFiles();
        private static readonly IEnumerable<FileDialogFilter> ImgFilter =
            FileDialogFilterComposer.Compose()
            .AddExtensions("KH2.IMG from the ISO or game disc", "IMG")
            .AddAllFiles();
        private static string ApplicationName = Utilities.GetApplicationName();

        public string Title => $"{_idxFileName ?? "no file loaded"} | {ApplicationName}";

        private Panel _itemPropertyPanel;
        private Stream _imgStream;
        private Img _img;
        private object _treeSelectedItem;
        private string _idxFileName;
        private string _imgFileName;

        public IdxImgViewModel(Panel itemPropertyPanel)
        {
            _itemPropertyPanel = itemPropertyPanel;

            OpenCommand = new RelayCommand(_ =>
            {
                try
                {
                    FileDialog.OnOpen(fileName =>
                    {
                        var baseName = Path.Combine(
                            Path.GetDirectoryName(fileName),
                            Path.GetFileNameWithoutExtension(fileName));
                        var idxFileName = baseName + ".idx";
                        var imgFileName = baseName + ".img";
                        if (!File.Exists(idxFileName))
                            FileDialog.OnOpen(fileName => idxFileName = fileName, IdxFilter);
                        if (!File.Exists(imgFileName))
                            FileDialog.OnOpen(fileName => imgFileName = fileName, ImgFilter);
                        OpenIdxImg(idxFileName, imgFileName);

                    }, Filter, "KH2.IDX");
                }
                catch (Exception ex)
                {
                    forms.MessageBox.Show(ex.Message, "Error",
                        forms.MessageBoxButtons.OK, forms.MessageBoxIcon.Error);
                }
            });
        }

        public RelayCommand OpenCommand { get; }

        public List<RootViewModel> Root { get; private set; }
        public object TreeSelectedItem
        {
            get => _treeSelectedItem;
            set
            {
                _itemPropertyPanel.Children.Clear();

                _treeSelectedItem = value;
                var itemPropertyControl = _treeSelectedItem switch
                {
                    FileViewModel _ => new FilePropertyView(),
                    _ => null,
                };
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTreeItemSelected));

                if (itemPropertyControl != null)
                    _itemPropertyPanel.Children.Add(itemPropertyControl);
            }
        }
        public bool IsTreeItemSelected => TreeSelectedItem != null;

        public void OpenIdxImg(string idxFileName, string imgFileName)
        {
            using var idxStream = File.OpenRead(idxFileName);
            if (!Idx.IsValid(idxStream))
                throw new ArgumentException($"The file '{idxFileName}' is not a valid IDX file.");
            var idx = Idx.Read(idxStream);

            _imgStream?.Dispose();
            _imgStream = File.OpenRead(imgFileName);
            _img = new Img(_imgStream, idx, false);

            Root = new List<RootViewModel>()
            {
                new RootViewModel(Path.GetFileName(idxFileName), idx, this)
            };

            _idxFileName = idxFileName;
            _imgFileName = imgFileName;

            OnPropertyChanged(nameof(Root));
            OnPropertyChanged(nameof(Title));
        }

        public Stream OpenFileFromIdx(string fileName) =>
            _img.FileOpen(fileName);

        public Stream OpenFileFromIdx(Idx.Entry idxEntry) =>
            _img.FileOpen(idxEntry);
    }
}
