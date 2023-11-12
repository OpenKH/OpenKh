using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    internal static class SelectExtensions
    {
        public static IEnumerable<(Type one, int index)> SelectWithIndex<Type>(this IEnumerable<Type> self) =>
            self.Select((one, index) => (one, index));
    }
}
