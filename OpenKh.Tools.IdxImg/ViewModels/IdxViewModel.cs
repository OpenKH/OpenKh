using OpenKh.Kh2;
using System.Collections.Generic;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    public class IdxViewModel : NodeViewModel
    {
        internal IdxViewModel(string name, Idx.Entry entry) :
            base(name, new List<EntryViewModel>())
        {
        }
    }
}
