using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Input;

namespace OpenKh.Tools.Kh2SoraikoTools
{
    public static class GeometricFunctions
    {
        public static class Matrix
        {
            public static Single GetSingle(double d)
            {
                double rounded = Math.Round(d, 7);
                float singleRound = (float)rounded;
                float singleNoRound = (float)d;

                if (Math.Abs(rounded - d) < Math.Abs((double)singleNoRound - d))
                    return singleRound;
                return singleNoRound;
            }
            public static void Decompose(Matrix4 input, out Vector3 rotate, out Vector3 translate, out Vector3 scale)
            {
                translate = input.ExtractTranslation();
                scale = input.ExtractScale();
                Quaternion q = input.ExtractRotation();

                Matrix4 mq = Matrix4.CreateFromQuaternion(q);

                double sy = Math.Sqrt(mq.M11 * mq.M11 + mq.M12 * mq.M12);

                bool singular = sy < 1e-6; // If

                if (!singular)
                {
                    rotate = new Vector3(
                        GetSingle(Math.Atan2(mq.M23, mq.M33)),
                        GetSingle(Math.Atan2(-mq.M13, sy)),
                        GetSingle(Math.Atan2(mq.M12, mq.M11)));
                }
                else
                {
                    rotate = new Vector3(
                        GetSingle(Math.Atan2(-mq.M32, mq.M22)),
                        GetSingle(Math.Atan2(-mq.M13, sy)),
                        0f);
                }
            }
            public static Matrix4 Recompose(Vector3 rotate, Vector3 translate, Vector3 scale)
            {
                return Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), rotate.X) *
                    Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), rotate.Y) *
                    Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), rotate.Z) *
                    Matrix4.CreateTranslation(translate.X, translate.Y, translate.Z) *
                    Matrix4.CreateScale(scale.X, scale.Y, scale.Z);
            }

        }
        public static class Vector
        {
            public static Vector3 RecalculateGlobalRotate(Vector3 rotate)
            {
                Vector3 newRotate;
                Vector3 v = Vector3.UnitX;
                v = Vector3.Transform(v, Matrix3.CreateRotationZ(rotate.Z));
                newRotate.Z = (float)Math.Atan2(v.Y, v.X);

                v = -Vector3.UnitZ;
                v = Vector3.Transform(v, Matrix3.CreateRotationX(rotate.X));
                newRotate.X = (float)Math.Atan2(v.Y, -v.Z);

                v = Vector3.UnitX;
                v = Vector3.Transform(v, Matrix3.CreateRotationY(rotate.Y));
                newRotate.Y = (float)Math.Atan2(-v.Z, v.X);

                return newRotate;
            }
        }

    }
}
