using System;
using System.IO;
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
    public struct WeightedVertex
    {
        public bool One;
        public Vector4 V;

        public WeightedVertex(Vector4 v4, bool last)
        {
            One = last;
            V = v4;
        }
    }
}
