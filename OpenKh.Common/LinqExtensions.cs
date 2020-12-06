using System;
using System.Collections.Generic;

namespace OpenKh.Common
{
    public static class LinqExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> source, T[] value)
            where T : IEquatable<T>
        {
            if (value.Length == 0)
                return 0;

            var index = 0;
            foreach (var item in source)
            {
                if (item.Equals(value[0]))
                {
                    var isEqual = true;
                    for (var i = 1; isEqual && i < value.Length; i++)
                        isEqual |= item.Equals(value[i]);

                    if (isEqual)
                        return index;
                }

                index++;
            }

            return -1;
        }
    }
}
