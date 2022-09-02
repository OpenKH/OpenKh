using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace OpenKh.Command.AnbMaker.Extensions
{
    internal static class AssimpExtensions
    {
        public static Vector3D ToAssimpVector3D(this Vector3 it) =>
            new Vector3D(it.X, it.Y, it.Z);

        public static Assimp.Quaternion ToAssimpQuaternion(this System.Numerics.Quaternion it) =>
            new Assimp.Quaternion(it.W, it.X, it.Y, it.Z);
    }
}
