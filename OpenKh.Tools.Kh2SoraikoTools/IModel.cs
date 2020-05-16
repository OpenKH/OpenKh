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
    public abstract class IModel
    {
        public int copyPass = 0;
        public int computingComplexity;
        public int Copies = 1;

        public WeightedVertex[] weightedVertices;
        public int[] influences;

        public Vector2[] textureCoordinates;

        public Vector3[] normals;
        public bool[] normalsNew;
        public bool[] meshesNormal;

        public Color4[] colors;
        public bool[] colorsNew;
        public bool[] meshesColor;

        public Vector3[] vertices;
        
        public int[] textures;
        public bool[] texturesAlpha;


        public int[][] MeshOffsets;

        static int instanceID = -1;
        public static int InstanceID
        {
            get
            {
                instanceID++;
                return instanceID;
            }
        }

        public string FileName;
        public string Directory;
        public string Name;
        public string Extension;


    }
}