using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Jiminy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Utils
{
    public struct BoundingBox
    {
        public Vector3 Maximum { get; set; }
        public Vector3 Minimum { get; set; }

        #region Workaround
        [EditorBrowsable(EditorBrowsableState.Never)] [Data] public float MinX { get => Minimum.X; set => Minimum = new Vector3(value, Minimum.Y, Minimum.Z); }
        [EditorBrowsable(EditorBrowsableState.Never)] [Data] public float MinY { get => Minimum.Y; set => Minimum = new Vector3(Minimum.X, value, Minimum.Z); }
        [EditorBrowsable(EditorBrowsableState.Never)] [Data] public float MinZ { get => Minimum.Z; set => Minimum = new Vector3(Minimum.X, Minimum.Y, value); }

        [EditorBrowsable(EditorBrowsableState.Never)] [Data] public float MaxX { get => Maximum.X; set => Maximum = new Vector3(value, Maximum.Y, Maximum.Z); }
        [EditorBrowsable(EditorBrowsableState.Never)] [Data] public float MaxY { get => Maximum.Y; set => Maximum = new Vector3(Maximum.X, value, Maximum.Z); }
        [EditorBrowsable(EditorBrowsableState.Never)] [Data] public float MaxZ { get => Maximum.Z; set => Maximum = new Vector3(Maximum.X, Maximum.Y, value); }
        #endregion

        public static readonly BoundingBox Invalid = new BoundingBox(
            new Vector3(float.MaxValue),
            new Vector3(float.MinValue)
        );

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

        public static BoundingBox Merge(BoundingBox box1, BoundingBox box2)
        {
            if (box1 == Invalid)
            {
                return box2;
            }
            if (box2 == Invalid)
            {
                return box1;
            }
            return new BoundingBox(
                Vector3.Min(box1.Minimum, box2.Minimum),
                Vector3.Max(box1.Maximum, box2.Maximum)
            );
        }

        public bool Equals(BoundingBox that)
            => Minimum == that.Minimum
            && Maximum == that.Maximum;

        public override bool Equals(object obj) => Equals((BoundingBox)obj);

        public override int GetHashCode() => Minimum.GetHashCode() + Maximum.GetHashCode();

        public static bool operator ==(BoundingBox left, BoundingBox right) => left.Equals(right);

        public static bool operator !=(BoundingBox left, BoundingBox right) => !(left == right);
    }
}
