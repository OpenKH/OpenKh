using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    internal static class GetAtOrNullExtension
    {
        public static T? GetAtOrNull<T>(this IList<T> source, int index) 
        {
            if ((uint)source.Count <= (uint)index)
            {
                return default;
            }
            else
            {
                return source[index];
            }
        }
    }
}
