using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Utils
{
    public struct BoundingBoxInt16
    {
        [Data] public Vector3Int16 Minimum { get; set; }
        [Data] public Vector3Int16 Maximum { get; set; }

        public static readonly BoundingBoxInt16 Invalid = new BoundingBoxInt16(
            new Vector3Int16(short.MaxValue),
            new Vector3Int16(short.MinValue)
        );

        public BoundingBoxInt16(Vector3Int16 min, Vector3Int16 max)
        {
            Minimum = min;
            Maximum = max;
        }

        public static BoundingBoxInt16 Merge(BoundingBoxInt16 box1, BoundingBoxInt16 box2)
        {
            if (box1 == Invalid)
            {
                return box2;
            }
            if (box2 == Invalid)
            {
                return box1;
            }
            return new BoundingBoxInt16(
                Vector3Int16.Minimize(box1.Minimum, box2.Minimum),
                Vector3Int16.Maximize(box1.Maximum, box2.Maximum)
            );
        }

        public BoundingBox ToBoundingBox()
        {
            return new BoundingBox(
                new Vector3(Minimum.X, Minimum.Y, Minimum.Z),
                new Vector3(Maximum.X, Maximum.Y, Maximum.Z)
            );
        }

        public BoundingBoxInt16 InflateWith(short inflate)
        {
            return new BoundingBoxInt16(
                new Vector3Int16((short)(Minimum.X - inflate), (short)(Minimum.Y - inflate), (short)(Minimum.Z - inflate)),
                new Vector3Int16((short)(Maximum.X + inflate), (short)(Maximum.Y + inflate), (short)(Maximum.Z + inflate))
            );
        }

        public bool Equals(BoundingBoxInt16 that)
            => Minimum == that.Minimum
            && Maximum == that.Maximum;

        public override bool Equals(object obj) => Equals((BoundingBoxInt16)obj);

        public override int GetHashCode() => Minimum.GetHashCode() + Maximum.GetHashCode();

        public static bool operator ==(BoundingBoxInt16 left, BoundingBoxInt16 right) => left.Equals(right);

        public static bool operator !=(BoundingBoxInt16 left, BoundingBoxInt16 right) => !(left == right);

        public override string ToString() => $"({Minimum}, {Maximum})";
    }
}
