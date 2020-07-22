using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3Int16 ToVector3Int16(this Vector3 vec) => new Vector3Int16(
            Convert.ToInt16(vec.X),
            Convert.ToInt16(vec.Y),
            Convert.ToInt16(vec.Z)
        );
    }
}
