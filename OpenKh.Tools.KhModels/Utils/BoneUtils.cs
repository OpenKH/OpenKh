using HelixToolkit.Wpf;
using OpenKh.Common;
using OpenKh.Kh1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using static OpenKh.Kh1.Mdls;

namespace OpenKh.Tools.KhModels.Utils
{
    public class BoneUtils
    {
        private static int CLUT_SIZE = 256 * 4; // 1024 = 256 colors, 4 bytes each

        public static GeometryModel3D getGeometryFromMdls(Mdls mdls, string? texturePath = null)
        {
            // MESH
            GeometryModel3D myGeometryModel = new GeometryModel3D();

            // GEOMETRY
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            Matrix4x4[] matrices = GetBoneMatrices(mdls);

            Point3DCollection myPositionCollection = new Point3DCollection();
            PointCollection myTextureCoordinatesCollection = new PointCollection();
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            for (int i = 0; i < mdls.Meshes.Count; i++)
            {
                Mdls.MdlsMesh mesh = mdls.Meshes[i];
                for (int j = 0; j < mesh.packet.StripHeaders.Count; j++)
                {
                    int currentVertex = 0;
                    foreach (Mdls.MdlsVertex vertex in mesh.packet.TriangleStrips[j])
                    {
                        Vector3 position = new Vector3(vertex.TranslateX, vertex.TranslateY, vertex.TranslateZ);
                        Vector3 finalPosition = Vector3.Transform(position, matrices[vertex.JointId]);
                        myPositionCollection.Add(new Point3D(finalPosition.X, finalPosition.Y, finalPosition.Z));

                        myTextureCoordinatesCollection.Add(new Point(vertex.TexCoordU, 1 - vertex.TexCoordV));

                        currentVertex++;

                        //if (j != 8)  continue;

                        if (currentVertex >= 3)
                        {
                            // Counterclockwise
                            if (mesh.packet.StripHeaders[j].Unknown == 0)
                            {
                                if (currentVertex % 2 == 0)
                                {
                                    myTriangleIndicesCollection.Add(vertex.Index);
                                    myTriangleIndicesCollection.Add(vertex.Index - 1);
                                    myTriangleIndicesCollection.Add(vertex.Index - 2);
                                }
                                else
                                {
                                    myTriangleIndicesCollection.Add(vertex.Index);
                                    myTriangleIndicesCollection.Add(vertex.Index - 2);
                                    myTriangleIndicesCollection.Add(vertex.Index - 1);
                                }
                            }
                            // Clockwise
                            else
                            {
                                if (currentVertex % 2 == 0)
                                {
                                    myTriangleIndicesCollection.Add(vertex.Index);
                                    myTriangleIndicesCollection.Add(vertex.Index - 2);
                                    myTriangleIndicesCollection.Add(vertex.Index - 1);
                                }
                                else
                                {
                                    myTriangleIndicesCollection.Add(vertex.Index);
                                    myTriangleIndicesCollection.Add(vertex.Index - 1);
                                    myTriangleIndicesCollection.Add(vertex.Index - 2);
                                }
                            }
                        }
                    }
                }
            }

            myMeshGeometry3D.Positions = myPositionCollection;
            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;


            /*// Create a collection of vertex positions for the MeshGeometry3D.
            Point3DCollection myPositionCollection = new Point3DCollection();
            foreach (Mdls.MdlsMesh mesh in mdls.Meshes)
            {
                foreach(Mdls.MdlsVertex vertex in mesh.packet.Vertices)
                {
                    Vector3 position = new Vector3(vertex.TranslateX, vertex.TranslateY, vertex.TranslateZ);
                    Vector3 finalPosition = Vector3.Transform(position, matrices[vertex.JointId]);
                    myPositionCollection.Add(new Point3D(finalPosition.X, finalPosition.Y, finalPosition.Z));
                }
            }
            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of texture coordinates for the MeshGeometry3D.
            PointCollection myTextureCoordinatesCollection = new PointCollection();
            foreach (Mdls.MdlsMesh mesh in mdls.Meshes)
            {
                foreach (Mdls.MdlsVertex vertex in mesh.packet.Vertices)
                {
                    myTextureCoordinatesCollection.Add(new Point(vertex.TexCoordU, 1 - vertex.TexCoordV));
                }
            }
            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            foreach (Mdls.MdlsMesh mesh in mdls.Meshes)
            {
                foreach (int[] face in mesh.packet.Faces)
                {
                    foreach(int faceIndex in face)
                        myTriangleIndicesCollection.Add(faceIndex);
                }
            }
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;*/

            myGeometryModel.Geometry = myMeshGeometry3D;

            // MATERIALS
            DiffuseMaterial myMaterial;
            try
            {
                //BitmapSource sauce = BitmapSource.Create(mdls.Images[0].Width, mdls.Images[0].Height, 96.0, 96.0, PixelFormats.Bgra32, null, mdls.Images[0].Data, mdls.Images[0].Width);
                //ImageSource imageSource = getBitmapAsDiffuseMaterial(getBitmapAsDiffuseMaterial(GetBitmapFromMdlsImage(mdls.Images[0])));
                //myMaterial = new DiffuseMaterial(new ImageBrush(imageSource));
                System.Drawing.Bitmap bitmap = GetBitmapFromMdlsImage(mdls.Images[0]);
                myMaterial = getBitmapAsDiffuseMaterial(bitmap);
            }
            catch (Exception e)
            {
                myMaterial = getDefaultMaterial();
            }
            myGeometryModel.Material = myMaterial;

            // Magic vertices so that the UV mapping goes from 0 to 1 instead of scaling to maximum existing
            myPositionCollection.Add(new Point3D(0, 0, 0));
            myPositionCollection.Add(new Point3D(0, 0, 0));
            myTextureCoordinatesCollection.Add(new Point(0, 0));
            myTextureCoordinatesCollection.Add(new Point(1, 1));

            return myGeometryModel;
        }

        public static DiffuseMaterial getDefaultMaterial()
        {
            LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
            myHorizontalGradient.StartPoint = new Point(0, 0.5);
            myHorizontalGradient.EndPoint = new Point(1, 0.5);
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
            myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

            return new DiffuseMaterial(myHorizontalGradient);
        }

        public static DiffuseMaterial getBitmapAsDiffuseMaterial(System.Drawing.Bitmap bitmap)
        {
            BitmapSource bitmapSource;
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                bitmapSource = BitmapFrame.Create(memory, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            }

            return new DiffuseMaterial(new ImageBrush(bitmapSource));
        }

        public static System.Drawing.Bitmap GetBitmapFromMdlsImage(MdlsImage image)
        {
            if (image.Data == null || image.Clut == null || image.Width <= 0 || image.Height <= 0)
            {
                throw new ArgumentException("Can't create bitmap");
            }

            if (image.Data.Length != image.Width * image.Height || image.Clut.Length != CLUT_SIZE)
            {
                throw new ArgumentException("Image data length or CLUT length is invalid");
            }

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (MemoryStream stream = new MemoryStream(image.Data))
            {
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x += 4)
                    {
                        int pixelIndex = stream.ReadInt32();

                        if ((pixelIndex & 31) >= 8)
                        {
                            if ((pixelIndex & 31) < 16)
                            {
                                pixelIndex += 8;                // +8 - 15 to +16 - 23
                            }
                            else if ((pixelIndex & 31) < 24)
                            {
                                pixelIndex -= 8;                // +16 - 23 to +8 - 15
                            }
                        }

                        pixelIndex <<= 2;

                        byte[] pixelGroup = BitConverter.GetBytes(pixelIndex);
                        System.Drawing.Color pixel1 = GetColorFromCLUT(image.Clut, pixelGroup[0]);
                        System.Drawing.Color pixel2 = GetColorFromCLUT(image.Clut, pixelGroup[1]);
                        System.Drawing.Color pixel3 = GetColorFromCLUT(image.Clut, pixelGroup[2]);
                        System.Drawing.Color pixel4 = GetColorFromCLUT(image.Clut, pixelGroup[3]);

                        bitmap.SetPixel(x, y, pixel1);
                        bitmap.SetPixel(x + 1, y, pixel2);
                        bitmap.SetPixel(x + 2, y, pixel3);
                        bitmap.SetPixel(x + 3, y, pixel4);
                    }
                }
            }
            return bitmap;
        }

        private static System.Drawing.Color GetColorFromCLUT(byte[] clut, int index)
        {
            int start = index * 4;
            byte red = clut[start];
            byte green = clut[start + 1];
            byte blue = clut[start + 2];
            byte alpha = (byte)((clut[start + 3] * 0xFF) >> 7);

            System.Drawing.Color color = System.Drawing.Color.FromArgb(alpha, red, green, blue);

            return color;
        }

        public static List<LinesVisual3D> GetSkeleton(Mdls mdls)
        {
            Matrix4x4[] matrices = GetBoneMatrices(mdls);

            List<Vector3> finalPositions = new List<Vector3>();

            List<LinesVisual3D> boneVisuals = new List<LinesVisual3D>();
            foreach (Mdls.MdlsJoint joint in mdls.Joints)
            {
                Vector3 parentTranslate = new Vector3(0, 0, 0);
                Vector3 finalTranslate = new Vector3(joint.TranslateX, joint.TranslateY, joint.TranslateZ);

                if (joint.ParentId != 0x000003FF)
                {
                    parentTranslate = finalPositions[(int)joint.ParentId];
                    finalTranslate = Vector3.Transform(new Vector3(joint.TranslateX, joint.TranslateY, joint.TranslateZ), matrices[joint.ParentId]);
                }

                finalPositions.Add(finalTranslate);

                var bone1Line = new LinesVisual3D();
                bone1Line.Points.Add(new System.Windows.Media.Media3D.Point3D(parentTranslate.X, parentTranslate.Y, parentTranslate.Z)); // Start point of the bone
                bone1Line.Points.Add(new System.Windows.Media.Media3D.Point3D(finalTranslate.X, finalTranslate.Y, finalTranslate.Z)); // End point of the bone
                bone1Line.Color = System.Windows.Media.Colors.Red; // Set the color of the line

                boneVisuals.Add(bone1Line);
            }

            return boneVisuals;
        }

        public static Matrix4x4[] GetBoneMatrices(Mdls mdls)
        {
            Vector3[] absTranslationList = new Vector3[mdls.Joints.Count];
            System.Numerics.Quaternion[] absRotationList = new System.Numerics.Quaternion[mdls.Joints.Count];

            for (int i = 0; i < mdls.Joints.Count; i++)
            {
                Mdls.MdlsJoint joint = mdls.Joints[i];
                int parentIndex = (int)joint.ParentId;
                System.Numerics.Quaternion absRotation;
                Vector3 absTranslation;

                if (parentIndex == -1 || parentIndex == 255)
                {
                    absRotation = System.Numerics.Quaternion.Identity;
                    absTranslation = Vector3.Zero;
                }
                else
                {
                    absRotation = absRotationList[parentIndex];
                    absTranslation = absTranslationList[parentIndex];
                }

                Vector3 localTranslation = Vector3.Transform(new Vector3(joint.TranslateX, joint.TranslateY, joint.TranslateZ), Matrix4x4.CreateFromQuaternion(absRotation));
                absTranslationList[i] = absTranslation + localTranslation;

                var localRotation = System.Numerics.Quaternion.Identity;
                if (joint.RotateX != 0)
                    localRotation *= (System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitZ, joint.RotateZ));
                if (joint.RotateY != 0)
                    localRotation *= (System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitY, joint.RotateY));
                if (joint.RotateZ != 0)
                    localRotation *= (System.Numerics.Quaternion.CreateFromAxisAngle(Vector3.UnitX, joint.RotateX));
                absRotationList[i] = absRotation * localRotation;
            }

            Matrix4x4[] matrices = new Matrix4x4[mdls.Joints.Count];
            for (int i = 0; i < mdls.Joints.Count; i++)
            {
                var absMatrix = Matrix4x4.Identity;
                absMatrix *= Matrix4x4.CreateFromQuaternion(absRotationList[i]);
                absMatrix *= Matrix4x4.CreateTranslation(absTranslationList[i]);
                matrices[i] = absMatrix;
            }

            return matrices;
        }
    }
}
