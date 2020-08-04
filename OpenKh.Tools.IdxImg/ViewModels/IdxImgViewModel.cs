using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class IdxImgViewModel
    {
        public IdxImgViewModel()
        {
        }

        public List<RootViewModel> Root { get; private set; }

        public void OpenIdx(string fileName)
        {
            var idx = File.OpenRead(fileName).Using(Idx.Read);
            Root = new List<RootViewModel>()
            {
                new RootViewModel("KH2.IDX", idx)
            };
        }
    }
}
