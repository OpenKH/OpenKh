using OpenKh.Kh2.Utils;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Kh2.Extensions
{
    public static class BoundingBoxInt16Extensions
    {
        public static BoundingBoxInt16 MergeAll(this IEnumerable<BoundingBoxInt16> list)
        {
            return list
                .Aggregate(
                    BoundingBoxInt16.Invalid,
                    BoundingBoxInt16.Merge
                );
        }
    }
}
