using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    internal static class ListExtensions
    {
        public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T>? source)
        {
            if (source != null)
            {
                list.AddRange(source);
            }
        }
    }
}
