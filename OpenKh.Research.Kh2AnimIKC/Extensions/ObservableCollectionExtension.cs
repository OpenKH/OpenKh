using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace OpenKh.Research.Kh2AnimIKC.Extensions
{
    static class ObservableCollectionExtension
    {
        internal static void ClearAndAddItems<T>(this ObservableCollection<T> self, IEnumerable<T> items)
        {
            self.Clear();
            items.ToList().ForEach(self.Add);
        }
    }
}
