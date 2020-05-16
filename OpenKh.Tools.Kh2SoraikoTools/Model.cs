using System;
using System.Runtime.InteropServices;
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
    public unsafe class Model : IModel
    {
        /* Official Constants found in KH2FM */
        public const float KH2FM_CONST_ROT_STEP_P_EX_PTR_0x11C = 0.2617993951f;


        public float rot_step = KH2FM_CONST_ROT_STEP_P_EX_PTR_0x11C;

        public static List<Model> Models = new List<Model>(0);

        /* As for KH2FM, allows to reuse the model data buffers for a file loaded twice. 
         * Only the skeleton and animations are instanced. */
        public Model RootCopy; 

        public int UniqueID;
        public Skeleton Skeleton;
        public Matrix4 LocalTransformMatrix;
        public Matrix4 LocationMatrix;

        public Vector3 Location;
        public float Scale = 1f;

        public float Rotation;
        public float DestRotation;

        public List<Model> AttachedChildren;
        public Model AttachedParent
        {
            get
            {
                return this.attachedParent;
            }
            set
            {
                if (value == null)
                {
                    if (this.attachedParent != null)
                    {
                        this.attachedParent.AttachedChildren.Remove(this);
                    }
                }
                else
                {
                    if (this.attachedParent != null && value.UniqueID == attachedParent.UniqueID)
                    {
                        new Exception("Evites de définir deux fois le même attachement !");
                        return;
                    }

                    value.AttachedChildren.Add(this);
                }
                this.attachedParent = value;
            }
        }

        Model attachedParent;
        public int AttachedParentBone;

        public Model(string filename)
        {
            if (File.Exists(StaticConstants.ProcessDirectory + @"\" + filename))
                filename = StaticConstants.ProcessDirectory + @"\" + filename;

            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.Extension = Path.GetExtension(filename);
            this.Directory = Path.GetDirectoryName(filename);
            this.FileName = filename;
            this.RootCopy = null;

            for (int i = 0; i < Models.Count;i++)
            {
                if (Models[i].FileName == filename)
                {
                    Models[i].Copies++;
                    this.RootCopy = Models[i];
                    break;
                }
            }
            Models.Add(this);
            this.UniqueID = IModel.InstanceID;

            if (this.RootCopy != null)
            {
                this.Skeleton = this.RootCopy.Skeleton.Clone();
                this.weightedVertices = this.RootCopy.weightedVertices;
                this.influences =  this.RootCopy.influences;
                this.textures = this.RootCopy.textures;
                this.texturesAlpha = this.RootCopy.texturesAlpha;
                this.MeshOffsets = this.RootCopy.MeshOffsets;
                this.vertices = new Vector3[this.RootCopy.vertices.Length];
                this.colors = this.RootCopy.colors;
                this.textureCoordinates = this.RootCopy.textureCoordinates;
                this.normals = this.RootCopy.normals;
                this.computingComplexity = this.RootCopy.computingComplexity;
            }


            this.attachedParent = null;
            this.AttachedParentBone = -1;
            this.AttachedChildren = new List<Model>(0);
        }



        public void Update(bool parentRequested)
        {
            if (!parentRequested && this.attachedParent != null)
                return;

            this.Rotation += (this.DestRotation- this.Rotation) * rot_step;

            this.LocationMatrix = Matrix4 .CreateTranslation(this.Location);
            this.LocalTransformMatrix = Matrix4 .CreateRotationY(this.Rotation);


            if (parentRequested)
            {
                Matrix4 mGlobal = this.AttachedParent.Skeleton.Matrices[AttachedParentBone];
                this.Skeleton.ComputeMatrices(mGlobal);
            }
            else
                this.Skeleton.ComputeMatrices(this.LocalTransformMatrix * this.LocationMatrix);


            /* UPDATING VERTICES */


            if (this.RootCopy == null || this.RootCopy.Copies< computingComplexity || this.copyPass% (this.RootCopy.Copies/10) == 0)
            {
                Matrix4[] transformsGlobal = new Matrix4[this.Skeleton.Joints.Length];
                for (int i = 0; i < this.Skeleton.Joints.Length; i++)
                    transformsGlobal[i] = this.Skeleton.Matrices[i];

                Array.Clear(vertices, 0, vertices.Length);

                int vIndex = 0;
                for (int i = 0; i < this.weightedVertices.Length; i++)
                {
                    Vector4 v4 = Vector4.Transform(this.weightedVertices[i].V, transformsGlobal[this.influences[i]]);

                    vertices[vIndex].X += v4.X;
                    vertices[vIndex].Y += v4.Y;
                    vertices[vIndex].Z += v4.Z;

                    if (this.weightedVertices[i].One)
                        vIndex++;
                }
            }

            copyPass++;
            for (int i = 0; i < this.AttachedChildren.Count; i++)
                this.AttachedChildren[i].Update(true);
        }
        
        public void Draw(bool parentRequested)
        {
            if (!parentRequested && this.attachedParent != null)
                return;
            
            GL.Enable(EnableCap.Texture2D);
            int vIndex = 0;
            int cIndex = 0;

            for (int i=0;i<this.MeshOffsets.Length;i++)
            {
                bool color = this.meshesColor[i];
                if (color)
                    GL.MatrixMode(MatrixMode.Color);
                else
                    GL.MatrixMode(MatrixMode.Modelview);

                GL.BindTexture(TextureTarget.Texture2D, this.textures[i]);

                bool enableAlpha = this.texturesAlpha[i];

                if (enableAlpha)
                {
                    GL.Enable(EnableCap.AlphaTest);
                    GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
                }

                GL.Begin(PrimitiveType.Triangles);
                if (color)
                    for (int j = 0; j < this.MeshOffsets[i][1]; j++)
                    {
                        if (this.colorsNew[vIndex])
                        {
                            GL.Color4(this.colors[cIndex]);
                            cIndex++;
                        }
                        GL.TexCoord2(this.textureCoordinates[vIndex]);
                        GL.Vertex3(this.vertices[vIndex]);
                        vIndex++;
                    }
                else
                    for (int j = 0; j < this.MeshOffsets[i][1]; j++)
                    {
                        GL.TexCoord2(this.textureCoordinates[vIndex]);
                        GL.Vertex3(this.vertices[vIndex]);
                        vIndex++;
                    }

                GL.End();

                if (enableAlpha)
                    GL.Disable(EnableCap.AlphaTest);
            }

            for (int i=0;i< this.AttachedChildren.Count;i++)
            {
                this.AttachedChildren[i].Draw(true);
            }
        }
    }
}