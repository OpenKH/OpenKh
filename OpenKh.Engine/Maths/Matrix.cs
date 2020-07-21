using System.Numerics;

namespace OpenKh.Engine.Maths
{
    public struct Matrix
    {
        public float M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44;

        public static Matrix Identity
        {
            get
            {
                return new Matrix()
                {
                    M11 = 1f,
                    M22 = 1f,
                    M33 = 1f,
                    M44 = 1f
                };
            }
        }

        public static Matrix RotationQuaternion(Quaternion rotation)
        {
            Matrix matrix = new Matrix();
            double x = (double)rotation.X;
            float num1 = (float)(x * x);
            double y = (double)rotation.Y;
            float num2 = (float)(y * y);
            double z = (double)rotation.Z;
            float num3 = (float)(z * z);
            float num4 = rotation.Y * rotation.X;
            float num5 = rotation.W * rotation.Z;
            float num6 = rotation.Z * rotation.X;
            float num7 = rotation.W * rotation.Y;
            float num8 = rotation.Z * rotation.Y;
            float num9 = rotation.W * rotation.X;
            matrix.M11 = (float)(1.0 - ((double)num3 + (double)num2) * 2.0);
            matrix.M12 = (float)(((double)num5 + (double)num4) * 2.0);
            matrix.M13 = (float)(((double)num6 - (double)num7) * 2.0);
            matrix.M14 = 0.0f;
            matrix.M21 = (float)(((double)num4 - (double)num5) * 2.0);
            matrix.M22 = (float)(1.0 - ((double)num3 + (double)num1) * 2.0);
            matrix.M23 = (float)(((double)num9 + (double)num8) * 2.0);
            matrix.M24 = 0.0f;
            matrix.M31 = (float)(((double)num7 + (double)num6) * 2.0);
            matrix.M32 = (float)(((double)num8 - (double)num9) * 2.0);
            matrix.M33 = (float)(1.0 - ((double)num2 + (double)num1) * 2.0);
            matrix.M34 = 0.0f;
            matrix.M41 = 0.0f;
            matrix.M42 = 0.0f;
            matrix.M43 = 0.0f;
            matrix.M44 = 1f;
            return matrix;
        }

        public static Matrix Translation(Vector3 amount)
        {
            return new Matrix()
            {
                M11 = 1f,
                M12 = 0.0f,
                M13 = 0.0f,
                M14 = 0.0f,
                M21 = 0.0f,
                M22 = 1f,
                M23 = 0.0f,
                M24 = 0.0f,
                M31 = 0.0f,
                M32 = 0.0f,
                M33 = 1f,
                M34 = 0.0f,
                M41 = amount.X,
                M42 = amount.Y,
                M43 = amount.Z,
                M44 = 1f
            };
        }

        public static Matrix operator *(Matrix left, Matrix right)
        {
            return new Matrix()
            {
                M11 = (float)((double)right.M21 * (double)left.M12 + (double)left.M11 * (double)right.M11 + (double)right.M31 * (double)left.M13 + (double)right.M41 * (double)left.M14),
                M12 = (float)((double)right.M22 * (double)left.M12 + (double)right.M12 * (double)left.M11 + (double)right.M32 * (double)left.M13 + (double)right.M42 * (double)left.M14),
                M13 = (float)((double)right.M23 * (double)left.M12 + (double)right.M13 * (double)left.M11 + (double)right.M33 * (double)left.M13 + (double)right.M43 * (double)left.M14),
                M14 = (float)((double)right.M24 * (double)left.M12 + (double)right.M14 * (double)left.M11 + (double)right.M34 * (double)left.M13 + (double)right.M44 * (double)left.M14),
                M21 = (float)((double)left.M22 * (double)right.M21 + (double)left.M21 * (double)right.M11 + (double)left.M23 * (double)right.M31 + (double)left.M24 * (double)right.M41),
                M22 = (float)((double)left.M22 * (double)right.M22 + (double)left.M21 * (double)right.M12 + (double)left.M23 * (double)right.M32 + (double)left.M24 * (double)right.M42),
                M23 = (float)((double)right.M23 * (double)left.M22 + (double)right.M13 * (double)left.M21 + (double)right.M33 * (double)left.M23 + (double)left.M24 * (double)right.M43),
                M24 = (float)((double)right.M24 * (double)left.M22 + (double)right.M14 * (double)left.M21 + (double)right.M34 * (double)left.M23 + (double)right.M44 * (double)left.M24),
                M31 = (float)((double)left.M32 * (double)right.M21 + (double)left.M31 * (double)right.M11 + (double)left.M33 * (double)right.M31 + (double)left.M34 * (double)right.M41),
                M32 = (float)((double)left.M32 * (double)right.M22 + (double)left.M31 * (double)right.M12 + (double)left.M33 * (double)right.M32 + (double)left.M34 * (double)right.M42),
                M33 = (float)((double)right.M23 * (double)left.M32 + (double)left.M31 * (double)right.M13 + (double)left.M33 * (double)right.M33 + (double)left.M34 * (double)right.M43),
                M34 = (float)((double)right.M24 * (double)left.M32 + (double)right.M14 * (double)left.M31 + (double)right.M34 * (double)left.M33 + (double)right.M44 * (double)left.M34),
                M41 = (float)((double)left.M42 * (double)right.M21 + (double)left.M41 * (double)right.M11 + (double)left.M43 * (double)right.M31 + (double)left.M44 * (double)right.M41),
                M42 = (float)((double)left.M42 * (double)right.M22 + (double)left.M41 * (double)right.M12 + (double)left.M43 * (double)right.M32 + (double)left.M44 * (double)right.M42),
                M43 = (float)((double)right.M23 * (double)left.M42 + (double)left.M41 * (double)right.M13 + (double)left.M43 * (double)right.M33 + (double)left.M44 * (double)right.M43),
                M44 = (float)((double)right.M24 * (double)left.M42 + (double)left.M41 * (double)right.M14 + (double)right.M34 * (double)left.M43 + (double)left.M44 * (double)right.M44)
            };
        }
    }
}
