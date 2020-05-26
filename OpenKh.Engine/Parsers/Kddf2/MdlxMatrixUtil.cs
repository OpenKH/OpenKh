using OpenKh.Engine.Maths;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
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

                    var localTranslation = Vector3.TransformCoordinate(new Vector3(oneBone.TranslationX, oneBone.TranslationY, oneBone.TranslationZ), Matrix.RotationQuaternion(absRotation));
                    absTranslationList[x] = absTranslation + localTranslation;

                    var localRotation = Quaternion.Identity;
                    if (oneBone.RotationX != 0) localRotation *= (Quaternion.RotationAxis(new Vector3(1, 0, 0), oneBone.RotationX));
                    if (oneBone.RotationY != 0) localRotation *= (Quaternion.RotationAxis(new Vector3(0, 1, 0), oneBone.RotationY));
                    if (oneBone.RotationZ != 0) localRotation *= (Quaternion.RotationAxis(new Vector3(0, 0, 1), oneBone.RotationZ));
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
    }
}
