using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using OpenKh.Kh2;
using OpenTK;

namespace OpenKh.Kh2.SrkAlternatives
{    
    public class Skeleton
    {
        public Bone[] Bones;
        public Matrix4[] Matrices;
        public Skeleton(int bones_count)
        {
            this.Bones = new Bone[bones_count];
            this.Matrices = new Matrix4[bones_count];
        }
        public void ComputeMatrices(Matrix4 globalTransform)
        {
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].TransformModel = Bones[i].TransformLocal * 1f;

                if (Bones[i].Parent > -1)
                    Bones[i].TransformModel *= Bones[Bones[i].Parent].TransformModel;

                Matrices[i] = Bones[i].TransformModel * globalTransform;
            }
        }
    }
}
