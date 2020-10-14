using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public static class MdlxMatrixUtil
    {
        /// <summary>
        /// Build intial T-pose matrices.
        /// </summary>
        public static Matrix4x4[] BuildTPoseMatrices(Mdlx.SubModel model, Matrix4x4 initialMatrix)
        {
            var boneList = model.Bones.ToArray();
            var matrices = new Matrix4x4[boneList.Length];
            {
                var absTranslationList = new Vector3[matrices.Length];
                var absRotationList = new Quaternion[matrices.Length];
                for (int x = 0; x < matrices.Length; x++)
                {
                    Quaternion absRotation;
                    Vector3 absTranslation;
                    var oneBone = boneList[x];
                    var parent = oneBone.Parent;
                    if (parent < 0)
                    {
                        absRotation = Quaternion.Identity;
                        absTranslation = Vector3.Zero;
                    }
                    else
                    {
                        absRotation = absRotationList[parent];
                        absTranslation = absTranslationList[parent];
                    }

                    var localTranslation = Vector3.Transform(new Vector3(oneBone.TranslationX, oneBone.TranslationY, oneBone.TranslationZ), Matrix4x4.CreateFromQuaternion(absRotation));
                    absTranslationList[x] = absTranslation + localTranslation;

                    var localRotation = Quaternion.Identity;
                    if (oneBone.RotationZ != 0) localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitZ, oneBone.RotationZ));
                    if (oneBone.RotationY != 0) localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitY, oneBone.RotationY));
                    if (oneBone.RotationX != 0) localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitX, oneBone.RotationX));
                    absRotationList[x] = absRotation * localRotation;
                }
                for (int x = 0; x < matrices.Length; x++)
                {
                    var absMatrix = initialMatrix;
                    absMatrix *= Matrix4x4.CreateFromQuaternion(absRotationList[x]);
                    absMatrix *= Matrix4x4.CreateTranslation(absTranslationList[x]);
                    matrices[x] = absMatrix;
                }
            }

            return matrices;
        }
    }
}
