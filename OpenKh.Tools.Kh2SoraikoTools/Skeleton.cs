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
    public class Skeleton
    {
        public Joint[] Joints;
        public Matrix4[] Matrices;

        public Skeleton(int joints_length)
        {
            this.Joints = new Joint[joints_length];
            this.Matrices = new Matrix4[joints_length];
        }

        public unsafe Skeleton Clone()
        {
            Skeleton output = new Skeleton(this.Joints.Length);
            for (int i = 0; i < Joints.Length; i++)
            {
                output.Joints[i] = new Joint(this.Joints[i].Name, this.Joints[i].TransformLocal);
                output.Joints[i].Parent = this.Joints[i].Parent;
            }
            
            return output;
        }

        public void ComputeMatrices(Matrix4 globalTransform)
        {
            for (int i = 0 ; i < Joints.Length; i++)
            {
                Joints[i].TransformModel = Joints[i].TransformLocal *1f;

                if (Joints[i].Parent > -1)
                    Joints[i].TransformModel *= Joints[Joints[i].Parent].TransformModel;

                Matrices[i] = Joints[i].TransformModel * globalTransform;
            }
        }
    }
}
