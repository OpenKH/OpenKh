using OpenKh.Engine.Maths;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public static class MdlxMatrixUtil
    {
        /// <summary>
        /// Build intial T-pose matrices.
        /// </summary>
        public static Matrix[] BuildTPoseMatrices(Mdlx.SubModel model, Matrix initialMatrix)
        {
            var boneList = model.Bones.ToArray();
            var matrices = new Matrix[boneList.Length];
            {
                var absTranslationList = new Vector3[matrices.Length];
                var absRotationList = new OpenKh.Engine.Maths.Quaternion[matrices.Length];
                for (int x = 0; x < matrices.Length; x++)
                {
                    OpenKh.Engine.Maths.Quaternion absRotation;
                    Vector3 absTranslation;
                    var oneBone = boneList[x];
                    var parent = oneBone.Parent;
                    if (parent < 0)
                    {
                        absRotation = OpenKh.Engine.Maths.Quaternion.Identity;
                        absTranslation = Vector3.Zero;
                    }
                    else
                    {
                        absRotation = absRotationList[parent];
                        absTranslation = absTranslationList[parent];
                    }

                    var localTranslation = TransformCoordinate(new Vector3(oneBone.TranslationX, oneBone.TranslationY, oneBone.TranslationZ), Matrix.RotationQuaternion(absRotation));
                    absTranslationList[x] = absTranslation + localTranslation;

                    var localRotation = OpenKh.Engine.Maths.Quaternion.Identity;
                    if (oneBone.RotationX != 0) localRotation *= (OpenKh.Engine.Maths.Quaternion.RotationAxis(new Vector3(1, 0, 0), oneBone.RotationX));
                    if (oneBone.RotationY != 0) localRotation *= (OpenKh.Engine.Maths.Quaternion.RotationAxis(new Vector3(0, 1, 0), oneBone.RotationY));
                    if (oneBone.RotationZ != 0) localRotation *= (OpenKh.Engine.Maths.Quaternion.RotationAxis(new Vector3(0, 0, 1), oneBone.RotationZ));
                    absRotationList[x] = localRotation * absRotation;
                }
                for (int x = 0; x < matrices.Length; x++)
                {
                    var absMatrix = initialMatrix;
                    absMatrix *= Matrix.RotationQuaternion(absRotationList[x]);
                    absMatrix *= Matrix.Translation(absTranslationList[x]);
                    matrices[x] = absMatrix;
                }
            }

            return matrices;
        }

        private static Vector3 TransformCoordinate(Vector3 coordinate, Matrix transformation)
        {
            Vector4 vector4 = new Vector4();
            vector4.X = (float)((double)transformation.M21 * (double)coordinate.Y + (double)transformation.M11 * (double)coordinate.X + (double)transformation.M31 * (double)coordinate.Z) + transformation.M41;
            vector4.Y = (float)((double)transformation.M22 * (double)coordinate.Y + (double)transformation.M12 * (double)coordinate.X + (double)transformation.M32 * (double)coordinate.Z) + transformation.M42;
            vector4.Z = (float)((double)transformation.M23 * (double)coordinate.Y + (double)transformation.M13 * (double)coordinate.X + (double)transformation.M33 * (double)coordinate.Z) + transformation.M43;
            float num = (float)(1.0 / ((double)transformation.M24 * (double)coordinate.Y + (double)transformation.M14 * (double)coordinate.X + (double)transformation.M34 * (double)coordinate.Z + (double)transformation.M44));
            vector4.W = num;
            Vector3 vector3 = new Vector3(vector4.X * num, vector4.Y * num, vector4.Z * num);
            return vector3;
        }
    }
}
