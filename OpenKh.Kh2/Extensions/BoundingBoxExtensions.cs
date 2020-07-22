using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2.Extensions
{
    public static class BoundingBoxExtensions
    {
        public static BoundingBox MergeAll(this IEnumerable<BoundingBox> list)
        {
            return list
                .Aggregate(
                    BoundingBox.Invalid,
                    BoundingBox.Merge
                );
        }
    }
}
