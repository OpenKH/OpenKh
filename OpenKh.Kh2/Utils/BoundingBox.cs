using OpenKh.Kh2.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Utils
{
    public struct BoundingBox
    {
        [Data] public Vector3 Maximum { get; set; }
        [Data] public Vector3 Minimum { get; set; }

        public BoundingBox(Vector3 minimum, Vector3 maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public static BoundingBox FromPoints(params Vector3[] points)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            foreach (var point in points)
            {
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }

            return new BoundingBox(min, max);
        }

        public override string ToString() => $"Minimum: {Minimum} Maximum: {Maximum}";

        public BoundingBoxInt16 ToBoundingBoxInt16()
        {
            return new BoundingBoxInt16(
                Minimum.ToVector3Int16(),
                Maximum.ToVector3Int16()
            );
        }
    }
}
