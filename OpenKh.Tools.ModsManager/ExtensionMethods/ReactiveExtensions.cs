using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.ExtensionMethods
{
    internal static class ReactiveExtensions
    {
        public static T AddTo<T>(this T self, ICollection<T> collection) where T : IDisposable
        {
            collection.Add(self);
            return self;
        }
    }
}
