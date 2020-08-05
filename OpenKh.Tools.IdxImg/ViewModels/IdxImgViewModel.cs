using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using OpenKh.Tools.IdxImg.Views;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Xe.Tools;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    class IdxImgViewModel : BaseNotifyPropertyChanged, IIdxManager, ITreeSelectedItem
    {
        private Panel _itemPropertyPanel;
        private Stream _imgStream;
        private Img _img;
        private object _treeSelectedItem;

        public IdxImgViewModel(Panel itemPropertyPanel)
        {
            _itemPropertyPanel = itemPropertyPanel;
        }

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
            var idx = File.OpenRead(idxFileName).Using(Idx.Read);

            _imgStream?.Dispose();
            _imgStream = File.OpenRead(imgFileName);
            _img = new Img(_imgStream, idx, false);

            Root = new List<RootViewModel>()
            {
                new RootViewModel("KH2.IDX", idx, this)
            };
        }

        public Stream OpenFileFromIdx(string fileName) =>
            _img.FileOpen(fileName);

        public Stream OpenFileFromIdx(Idx.Entry idxEntry) =>
            _img.FileOpen(idxEntry);
    }
}
