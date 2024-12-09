using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    internal static class SelectExtensions
    {
        public static IEnumerable<(Type one, int index)> SelectWithIndex<Type>(this IEnumerable<Type> self) =>
            self.Select((one, index) => (one, index));
    }
}
