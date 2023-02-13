using System;

namespace OpenKh.Tools.Kh2MdlxEditor.Utils
{
    public class MathUtils
    {
        // TO EULER ANGLES
        public static float toEuler(float radian)
        {
            return radian *= (float)(180 / Math.PI);
        }
        public static System.Numerics.Vector3 toEuler(System.Numerics.Vector3 radianRotation)
        {
            return new System.Numerics.Vector3(toEuler(radianRotation.X), toEuler(radianRotation.Y), toEuler(radianRotation.Z));
        }

        // TO RADIANS
        public static float toRadian(float radian)
        {
            return radian *= (float)(Math.PI / 180);
        }
        public static System.Numerics.Vector3 toRadian(System.Numerics.Vector3 eulerRotation)
        {
            return new System.Numerics.Vector3(toRadian(eulerRotation.X), toRadian(eulerRotation.Y), toRadian(eulerRotation.Z));
        }

        // QUATERNIONS
        public static System.Numerics.Quaternion ToQuaternion(System.Numerics.Vector3 v)
        {

            float cy = (float)Math.Cos(v.Z * 0.5);
            float sy = (float)Math.Sin(v.Z * 0.5);
            float cp = (float)Math.Cos(v.Y * 0.5);
            float sp = (float)Math.Sin(v.Y * 0.5);
            float cr = (float)Math.Cos(v.X * 0.5);
            float sr = (float)Math.Sin(v.X * 0.5);

            return new System.Numerics.Quaternion
            {
                W = (cr * cp * cy + sr * sp * sy),
                X = (sr * cp * cy - cr * sp * sy),
                Y = (cr * sp * cy + sr * cp * sy),
                Z = (cr * cp * sy - sr * sp * cy)
            };

        }

        public static System.Numerics.Vector3 ToEulerAngles(System.Numerics.Quaternion q)
        {
            System.Numerics.Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
    }
}
