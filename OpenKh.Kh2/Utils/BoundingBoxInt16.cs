using OpenKh.Kh2.Jiminy;
using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Utils
{
    public struct BoundingBoxInt16
    {
        [Data] public Vector3Int16 Minimum { get; set; }
        [Data] public Vector3Int16 Maximum { get; set; }

        public BoundingBoxInt16(Vector3Int16 min, Vector3Int16 max)
        {
            Minimum = min;
            Maximum = max;
        }

        public static BoundingBoxInt16 Merge(BoundingBoxInt16 box1, BoundingBoxInt16 box2)
        {
            return new BoundingBoxInt16(
                Vector3Int16.Minimize(box1.Minimum, box2.Minimum),
                Vector3Int16.Maximize(box1.Maximum, box2.Maximum)
            );
        }
    }
}
