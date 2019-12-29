using System;

namespace OpenKh.Engine.Maths
{
    public struct Vector3
    {
        public float X, Y, Z;

        public static Vector3 Zero
        {
            get
            {
                return new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public float Length()
        {
            double y = (double)this.Y;
            double x = (double)this.X;
            double z = (double)this.Z;
            double num1 = x;
            double num2 = num1 * num1;
            double num3 = y;
            double num4 = num3 * num3;
            double num5 = num2 + num4;
            double num6 = z;
            double num7 = num6 * num6;
            return (float)Math.Sqrt(num5 + num7);
        }

        public static void Normalize(ref Vector3 vector, out Vector3 result)
        {
            Vector3 vector3 = vector;
            result = vector3;
            result.Normalize();
        }

        public void Normalize()
        {
            float num1 = this.Length();
            if ((double)num1 == 0.0)
                return;
            float num2 = 1f / num1;
            this.X *= num2;
            this.Y *= num2;
            this.Z *= num2;
        }

        public static Vector3 TransformCoordinate(Vector3 coordinate, Matrix transformation)
        {
            Vector4 vector4 = new Vector4();
            vector4.X = (float)((double)transformation.M21 * (double)coordinate.Y + (double)transformation.M11 * (double)coordinate.X + (double)transformation.M31 * (double)coordinate.Z) + transformation.M41;
            vector4.Y = (float)((double)transformation.M22 * (double)coordinate.Y + (double)transformation.M12 * (double)coordinate.X + (double)transformation.M32 * (double)coordinate.Z) + transformation.M42;
            vector4.Z = (float)((double)transformation.M23 * (double)coordinate.Y + (double)transformation.M13 * (double)coordinate.X + (double)transformation.M33 * (double)coordinate.Z) + transformation.M43;
            float num = (float)(1.0 / ((double)transformation.M24 * (double)coordinate.Y + (double)transformation.M14 * (double)coordinate.X + (double)transformation.M34 * (double)coordinate.Z + (double)transformation.M44));
            vector4.W = num;
            Vector3 vector3;
            vector3.X = vector4.X * num;
            vector3.Y = vector4.Y * num;
            vector3.Z = vector4.Z * num;
            return vector3;
        }

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            Vector3 vector3;
            vector3.X = left.X + right.X;
            vector3.Y = left.Y + right.Y;
            vector3.Z = left.Z + right.Z;
            return vector3;
        }

        public static Vector3 operator *(Vector3 vector, float scale)
        {
            Vector3 vector3;
            vector3.X = vector.X * scale;
            vector3.Y = vector.Y * scale;
            vector3.Z = vector.Z * scale;
            return vector3;
        }
    }
}
