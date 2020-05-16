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
    public class Joint
    {
        public int Parent;
        public string Name;

        public Matrix4 TransformLocal;
        public Matrix4 TransformModel;
        
        public Joint(string name, Matrix4  matrice)
        {
            //Children = null;
            //Index = -1;
            Parent = -1;
            Name = name;
            
            TransformLocal = matrice * 1f;
            TransformModel = matrice * 1f;
        }
    }
}
