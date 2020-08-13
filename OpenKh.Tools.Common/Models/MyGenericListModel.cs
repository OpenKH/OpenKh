using System.Collections;
using System.Collections.Generic;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.Common.Models
{
    public class MyGenericListModel<T> : GenericListModel<T>, IEnumerable<T>
    {
        public MyGenericListModel(IEnumerable<T> list) : base(list)
        {
        }

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
        protected override T OnNewItem() => throw new System.NotImplementedException();
    }
}
