using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

namespace OpenKh.Kh2.SrkAlteranatives
{
    public struct Bone
    {
        public int Parent;
        public Matrix4 TransformLocal {
            get
            {
                return
                    Matrix4.CreateFromAxisAngle(Vector3.UnitX, RotateX) *
                    Matrix4.CreateFromAxisAngle(Vector3.UnitY, RotateY) *
                    Matrix4.CreateFromAxisAngle(Vector3.UnitZ, RotateZ) *
                    Matrix4.CreateTranslation(TranslateX, TranslateY, TranslateZ) *
                    Matrix4.CreateScale(ScaleX, ScaleY, ScaleZ);
            }
        }

        public Matrix4 TransformModel;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float RotateX;
        public float RotateY;
        public float RotateZ;
        public float TranslateX;
        public float TranslateY;
        public float TranslateZ;

    }
}
