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

        public static Vector3 ToDotNetVector3(this Vector3D it) =>
            new Vector3(it.X, it.Y, it.Z);

        public static System.Numerics.Quaternion ToDotNetQuaternion(this Assimp.Quaternion it) =>
            new System.Numerics.Quaternion(it.X, it.Y, it.Z, it.W);


        public static Assimp.Quaternion GetInterpolatedQuaternion(this List<QuaternionKey> keys, double time)
        {
            if (keys.Any())
            {
                if (time < keys.First().Time)
                {
                    return keys.First().Value;
                }
                if (keys.Last().Time < time)
                {
                    return keys.Last().Value;
                }

                for (int x = 0, cx = keys.Count - 1; x < cx; x++)
                {
                    var time0 = keys[x].Time;
                    var time1 = keys[x + 1].Time;

                    if (time0 <= time && time <= time1)
                    {
                        float ratio = (float)((time - time0) / (time1 - time0));

                        return Assimp.Quaternion.Slerp(
                            keys[x].Value,
                            keys[x + 1].Value,
                            ratio
                        );
                    }
                }
            }

            return new Assimp.Quaternion(1, 0, 0, 0);
        }

        public static Vector3D GetInterpolatedVector(this List<VectorKey> keys, double time)
        {
            if (keys.Any())
            {
                if (time < keys.First().Time)
                {
                    return keys.First().Value;
                }
                if (keys.Last().Time < time)
                {
                    return keys.Last().Value;
                }

                for (int x = 0, cx = keys.Count - 1; x < cx; x++)
                {
                    var time0 = keys[x].Time;
                    var time1 = keys[x + 1].Time;

                    if (time0 <= time && time <= time1)
                    {
                        float ratio = (float)((time - time0) / (time1 - time0));

                        return (keys[x].Value * (1f - ratio)) + (keys[x + 1].Value * ratio);
                    }
                }
            }

            return new Vector3D(0, 0, 0);
        }
    }
}
