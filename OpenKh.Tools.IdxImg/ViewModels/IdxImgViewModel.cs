using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    class IdxImgViewModel : IIdxManager
    {
        private Stream _imgStream;
        private Img _img;

        public IdxImgViewModel()
        {
        }

        public List<RootViewModel> Root { get; private set; }
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
