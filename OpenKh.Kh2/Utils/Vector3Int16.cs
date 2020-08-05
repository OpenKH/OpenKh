using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Utils
{
    public struct Vector3Int16
    {
        [Data] public short X { get; set; }
        [Data] public short Y { get; set; }
        [Data] public short Z { get; set; }

        public Vector3Int16(short value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Vector3Int16(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3Int16 Minimize(Vector3Int16 vec1, Vector3Int16 vec2)
        {
            return new Vector3Int16(
                Math.Min(vec1.X, vec2.X),
                Math.Min(vec1.Y, vec2.Y),
                Math.Min(vec1.Z, vec2.Z)
            );
        }

        public static Vector3Int16 Maximize(Vector3Int16 vec1, Vector3Int16 vec2)
        {
            return new Vector3Int16(
                Math.Max(vec1.X, vec2.X),
                Math.Max(vec1.Y, vec2.Y),
                Math.Max(vec1.Z, vec2.Z)
            );
        }

        public bool Equals(Vector3Int16 that)
            => X == that.X
            && Y == that.Y
            && Z == that.Z;

        public override bool Equals(object obj) => Equals((Vector3Int16)obj);

        public override int GetHashCode() => X + Y + Z;

        public static bool operator ==(Vector3Int16 left, Vector3Int16 right) => left.Equals(right);

        public static bool operator !=(Vector3Int16 left, Vector3Int16 right) => !(left == right);

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
