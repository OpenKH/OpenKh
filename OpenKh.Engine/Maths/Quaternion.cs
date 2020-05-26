using System;

namespace OpenKh.Engine.Maths
{
    public struct Quaternion
    {
        public float X, Y, Z, W;

        public static Quaternion Identity
        {
            get
            {
                return new Quaternion()
                {
                    X = 0.0f,
                    Y = 0.0f,
                    Z = 0.0f,
                    W = 1f
                };
            }
        }

        public static Quaternion RotationAxis(Vector3 axis, float angle)
        {
            Quaternion quaternion = new Quaternion();
            Vector3.Normalize(ref axis, out axis);
            float num1 = angle * 0.5f;
            float num2 = (float)Math.Sin((double)num1);
            float num3 = (float)Math.Cos((double)num1);
            quaternion.X = axis.X * num2;
            quaternion.Y = axis.Y * num2;
            quaternion.Z = axis.Z * num2;
            quaternion.W = num3;
            return quaternion;
        }

        public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            Quaternion quaternion = new Quaternion();
            float x1 = left.X;
            float y1 = left.Y;
            float z1 = left.Z;
            float w1 = left.W;
            float x2 = right.X;
            float y2 = right.Y;
            float z2 = right.Z;
            float w2 = right.W;
            quaternion.X = (float)((double)x2 * (double)w1 + (double)w2 * (double)x1 + (double)y2 * (double)z1 - (double)z2 * (double)y1);
            quaternion.Y = (float)((double)y2 * (double)w1 + (double)w2 * (double)y1 + (double)z2 * (double)x1 - (double)x2 * (double)z1);
            quaternion.Z = (float)((double)z2 * (double)w1 + (double)w2 * (double)z1 + (double)x2 * (double)y1 - (double)y2 * (double)x1);
            quaternion.W = (float)((double)w2 * (double)w1 - ((double)y2 * (double)y1 + (double)x2 * (double)x1 + (double)z2 * (double)z1));
            return quaternion;
        }
    }
}
