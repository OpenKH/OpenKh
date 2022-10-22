using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenKh.Tools.Kh2MdlxEditor.Utils
{
    internal class Viewport3DUtils
    {
        public static PerspectiveCamera getDefaultCamera(int distance = 500)
        {
            PerspectiveCamera myPCamera = new PerspectiveCamera();
            myPCamera.Position = new Point3D(0, 0, distance);
            myPCamera.LookDirection = new Vector3D(0, 0, -1);
            myPCamera.FieldOfView = 60;
            
            return myPCamera;
        }
        public static Vector3D getVectorToTarget(Point3D position, Point3D targetPosition = new Point3D())
        {
            Vector3D vector = new Vector3D(position.X, position.Y, position.Z);
            Vector3D targetVector = new Vector3D(targetPosition.X, targetPosition.Y, targetPosition.Z);
            return getVectorToTarget(vector, targetVector);
        }
        public static Vector3D getVectorToTarget(Vector3D position, Vector3D targetPosition = new Vector3D())
        {
            return -(position - targetPosition);
        }

        public static GeometryModel3D getGeometryFromGroup(ModelSkeletal.SkeletalGroup group, ModelTexture? textureFile = null)
        {
            // MESH
            GeometryModel3D myGeometryModel = new GeometryModel3D();

            // GEOMETRY
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // Create a collection of vertex positions for the MeshGeometry3D.
            Point3DCollection myPositionCollection = new Point3DCollection();
            foreach (ModelCommon.UVBVertex vertex in group.Mesh.Vertices)
            {
                myPositionCollection.Add(new Point3D(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
            }
            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of texture coordinates for the MeshGeometry3D.
            PointCollection myTextureCoordinatesCollection = new PointCollection();
            foreach (ModelCommon.UVBVertex vertex in group.Mesh.Vertices)
            {
                myTextureCoordinatesCollection.Add(new Point(vertex.U, vertex.V));
            }
            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            foreach (List<int> triangle in group.Mesh.Triangles)
            {
                foreach (int triangleVertex in triangle)
                    myTriangleIndicesCollection.Add(triangleVertex);
            }
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            myGeometryModel.Geometry = myMeshGeometry3D;

            // MATERIALS
            DiffuseMaterial myMaterial;
            try
            {
                int textureIndex = (int)group.Header.TextureIndex;
                if (textureFile == null || textureFile?.Images?.Count == null || textureFile.Images.Count < textureIndex)
                {
                    myMaterial = getDefaultMaterial();
                }
                else
                {
                    ModelTexture.Texture texture = textureFile.Images[textureIndex];
                    ImageSource imageSource = texture.GetBimapSource();
                    myMaterial = new DiffuseMaterial(new ImageBrush(imageSource));
                }
            }
            catch (Exception e)
            {
                myMaterial = getDefaultMaterial();
            }

            myGeometryModel.Material = myMaterial;

            return myGeometryModel;
        }

        public static List<GeometryModel3D> getGeometryFromModel(ModelSkeletal modelFile, ModelTexture? textureFile = null)
        {
            List<GeometryModel3D> geometryList = new List<GeometryModel3D>();
            foreach(ModelSkeletal.SkeletalGroup group in modelFile.Groups)
            {
                geometryList.Add(getGeometryFromGroup(group, textureFile));
            }

            return geometryList;
        }

        public static Viewport3D get3DViewport(ModelSkeletal modelSkel, ModelCollision? collisionFile = null)
        {
            Viewport3D myViewport3D = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();

            PerspectiveCamera myPCamera = new PerspectiveCamera();
            myPCamera.Position = new Point3D(0, 0, 500);
            myPCamera.LookDirection = new Vector3D(0, 0, -1);
            myPCamera.FieldOfView = 60;
            myViewport3D.Camera = myPCamera;

            AmbientLight myAmbientLight = new AmbientLight(System.Windows.Media.Brushes.White.Color);
            myModel3DGroup.Children.Add(myAmbientLight);

            foreach (ModelSkeletal.SkeletalGroup mesh in modelSkel.Groups)
            {
                MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

                // Create a collection of vertex positions for the MeshGeometry3D.
                Point3DCollection myPositionCollection = new Point3DCollection();
                foreach (ModelCommon.UVBVertex vertex in mesh.Mesh.Vertices)
                {
                    myPositionCollection.Add(new Point3D(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
                }
                /*myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));
                myPositionCollection.Add(new Point3D(0.5, -0.5, 0.5));
                myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
                myPositionCollection.Add(new Point3D(0.5, 0.5, 0.5));
                myPositionCollection.Add(new Point3D(-0.5, 0.5, 0.5));
                myPositionCollection.Add(new Point3D(-0.5, -0.5, 0.5));*/
                myMeshGeometry3D.Positions = myPositionCollection;

                // Create a collection of texture coordinates for the MeshGeometry3D.
                PointCollection myTextureCoordinatesCollection = new PointCollection();
                foreach (ModelCommon.UVBVertex vertex in mesh.Mesh.Vertices)
                {
                    myTextureCoordinatesCollection.Add(new Point(vertex.U, vertex.V));
                }
                /*myTextureCoordinatesCollection.Add(new Point(0, 0));
                myTextureCoordinatesCollection.Add(new Point(1, 0));
                myTextureCoordinatesCollection.Add(new Point(1, 1));
                myTextureCoordinatesCollection.Add(new Point(1, 1));
                myTextureCoordinatesCollection.Add(new Point(0, 1));
                myTextureCoordinatesCollection.Add(new Point(0, 0));*/
                myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

                // Create a collection of triangle indices for the MeshGeometry3D.
                Int32Collection myTriangleIndicesCollection = new Int32Collection();
                foreach (List<int> triangle in mesh.Mesh.Triangles)
                {
                    foreach (int triangleVertex in triangle)
                        myTriangleIndicesCollection.Add(triangleVertex);
                }
                /*myTriangleIndicesCollection.Add(0);
                myTriangleIndicesCollection.Add(1);
                myTriangleIndicesCollection.Add(2);
                myTriangleIndicesCollection.Add(3);
                myTriangleIndicesCollection.Add(4);
                myTriangleIndicesCollection.Add(5);*/
                myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

                GeometryModel3D myGeometryModel = new GeometryModel3D();
                // Apply the mesh to the geometry model.
                myGeometryModel.Geometry = myMeshGeometry3D;

                // The material specifies the material applied to the 3D object. In this sample a
                // linear gradient covers the surface of the 3D object.

                // Create a horizontal linear gradient with four stops.
                LinearGradientBrush myHorizontalGradient = new LinearGradientBrush();
                myHorizontalGradient.StartPoint = new Point(0, 0.5);
                myHorizontalGradient.EndPoint = new Point(1, 0.5);
                myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
                myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
                myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
                myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

                // Define material and apply to the mesh geometries.
                DiffuseMaterial myMaterial = new DiffuseMaterial(myHorizontalGradient);
                myGeometryModel.Material = myMaterial;
                //myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(100, 200, 175, 0)));

                // Apply a transform to the object. In this sample, a rotation transform is applied,
                // rendering the 3D object rotated.
                /*RotateTransform3D myRotateTransform3D = new RotateTransform3D();
                AxisAngleRotation3D myAxisAngleRotation3d = new AxisAngleRotation3D();
                myAxisAngleRotation3d.Axis = new Vector3D(0, 3, 0);
                myAxisAngleRotation3d.Angle = 40;
                myRotateTransform3D.Rotation = myAxisAngleRotation3d;
                myGeometryModel.Transform = myRotateTransform3D;*/

                // Add the geometry model to the model group.
                myModel3DGroup.Children.Add(myGeometryModel);
            }

            //--------------------------------------------------------------------------------------
            // TEST
            if (collisionFile != null)
            {
                foreach (ObjectCollision collision in collisionFile.EntryList)
                {
                    if (collision.Bone == 16384 || collision.Bone == 2)
                        myModel3DGroup.Children.Add(getSquare(collision.Radius, new Vector3D(collision.PositionX, collision.PositionY, collision.PositionZ)));
                }
            }
            //myModel3DGroup.Children.Add(getSquare(90));
            //--------------------------------------------------------------------------------------

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            //
            myViewport3D.Children.Add(myModelVisual3D);

            // Apply the viewport to the page so it will be rendered.
            //this.Content = myViewport3D;
            return myViewport3D;
        }

        public static void addTri(Int32Collection myTriangleIndicesCollection, int i1, int i2, int i3)
        {
            myTriangleIndicesCollection.Add(i1);
            myTriangleIndicesCollection.Add(i2);
            myTriangleIndicesCollection.Add(i3);
        }

        public static GeometryModel3D getCube(int size, Vector3D position, Color color)
        {
            GeometryModel3D cube = getCube(size, position);
            cube.Material = new DiffuseMaterial(new SolidColorBrush(color));

            return cube;
        }
        public static GeometryModel3D getCube(int size, Vector3D position = new Vector3D())
        {
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // Create a collection of vertex positions for the MeshGeometry3D.
            Point3DCollection myPositionCollection = new Point3DCollection();

            myPositionCollection.Add(new Point3D(position.X - size, position.Y - size, position.Z - size));
            myPositionCollection.Add(new Point3D(position.X + size, position.Y - size, position.Z - size));
            myPositionCollection.Add(new Point3D(position.X - size, position.Y + size, position.Z - size));
            myPositionCollection.Add(new Point3D(position.X + size, position.Y + size, position.Z - size));
            myPositionCollection.Add(new Point3D(position.X - size, position.Y - size, position.Z + size));
            myPositionCollection.Add(new Point3D(position.X + size, position.Y - size, position.Z + size));
            myPositionCollection.Add(new Point3D(position.X - size, position.Y + size, position.Z + size));
            myPositionCollection.Add(new Point3D(position.X + size, position.Y + size, position.Z + size));

            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            
            addTri(myTriangleIndicesCollection, 2, 3, 1); // Back
            addTri(myTriangleIndicesCollection, 2, 1, 0); // Back
            addTri(myTriangleIndicesCollection, 7, 1, 3); // Left
            addTri(myTriangleIndicesCollection, 7, 5, 1); // Left
            addTri(myTriangleIndicesCollection, 6, 5, 7); // Front
            addTri(myTriangleIndicesCollection, 6, 4, 5); // Front
            addTri(myTriangleIndicesCollection, 2, 4, 6); // Left
            addTri(myTriangleIndicesCollection, 2, 0, 4); // Left
            addTri(myTriangleIndicesCollection, 2, 7, 3); // Up
            addTri(myTriangleIndicesCollection, 2, 6, 7); // Up
            addTri(myTriangleIndicesCollection, 0, 1, 5); // Down
            addTri(myTriangleIndicesCollection, 0, 5, 4); // Down

            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            GeometryModel3D myGeometryModel = new GeometryModel3D();
            myGeometryModel.Geometry = myMeshGeometry3D;
            myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)));

            return myGeometryModel;
        }
        public static GeometryModel3D getCube(int radius, int height, Vector3D position, Color color)
        {
            GeometryModel3D cube = getCube(radius, height, position);
            cube.Material = new DiffuseMaterial(new SolidColorBrush(color));

            return cube;
        }
        public static GeometryModel3D getCube(int radius, int height, Vector3D position = new Vector3D())
        {
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // Create a collection of vertex positions for the MeshGeometry3D.
            Point3DCollection myPositionCollection = new Point3DCollection();

            myPositionCollection.Add(new Point3D(position.X - radius, position.Y - height, position.Z - radius));
            myPositionCollection.Add(new Point3D(position.X + radius, position.Y - height, position.Z - radius));
            myPositionCollection.Add(new Point3D(position.X - radius, position.Y + height, position.Z - radius));
            myPositionCollection.Add(new Point3D(position.X + radius, position.Y + height, position.Z - radius));
            myPositionCollection.Add(new Point3D(position.X - radius, position.Y - height, position.Z + radius));
            myPositionCollection.Add(new Point3D(position.X + radius, position.Y - height, position.Z + radius));
            myPositionCollection.Add(new Point3D(position.X - radius, position.Y + height, position.Z + radius));
            myPositionCollection.Add(new Point3D(position.X + radius, position.Y + height, position.Z + radius));

            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();

            addTri(myTriangleIndicesCollection, 2, 3, 1); // Back
            addTri(myTriangleIndicesCollection, 2, 1, 0); // Back
            addTri(myTriangleIndicesCollection, 7, 1, 3); // Left
            addTri(myTriangleIndicesCollection, 7, 5, 1); // Left
            addTri(myTriangleIndicesCollection, 6, 5, 7); // Front
            addTri(myTriangleIndicesCollection, 6, 4, 5); // Front
            addTri(myTriangleIndicesCollection, 2, 4, 6); // Left
            addTri(myTriangleIndicesCollection, 2, 0, 4); // Left
            addTri(myTriangleIndicesCollection, 2, 7, 3); // Up
            addTri(myTriangleIndicesCollection, 2, 6, 7); // Up
            addTri(myTriangleIndicesCollection, 0, 1, 5); // Down
            addTri(myTriangleIndicesCollection, 0, 5, 4); // Down

            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            GeometryModel3D myGeometryModel = new GeometryModel3D();
            myGeometryModel.Geometry = myMeshGeometry3D;
            myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)));

            return myGeometryModel;
        }

        public static GeometryModel3D getSquare(int size, Vector3D position = new Vector3D())
        {
            MeshGeometry3D myMeshGeometry3D = new MeshGeometry3D();

            // Create a collection of vertex positions for the MeshGeometry3D.
            Point3DCollection myPositionCollection = new Point3DCollection();
            myPositionCollection.Add(new Point3D(-size, -size, size));
            myPositionCollection.Add(new Point3D(size, -size, size));
            myPositionCollection.Add(new Point3D(size, size, size));

            myPositionCollection.Add(new Point3D(size, size, size));
            myPositionCollection.Add(new Point3D(-size, size, size));
            myPositionCollection.Add(new Point3D(-size, -size, size));

            for (int i = 0; i < myPositionCollection.Count; i++)
            {
                Point3D point = myPositionCollection[i];
                point.X += position.X;
                point.Y += position.Y;
                point.Z += position.Z;
            }

            myMeshGeometry3D.Positions = myPositionCollection;

            // Create a collection of texture coordinates for the MeshGeometry3D.
            PointCollection myTextureCoordinatesCollection = new PointCollection();
            myTextureCoordinatesCollection.Add(new Point(0, 0));
            myTextureCoordinatesCollection.Add(new Point(0, 0));
            myTextureCoordinatesCollection.Add(new Point(1, 1));
            myTextureCoordinatesCollection.Add(new Point(0, 0));
            myTextureCoordinatesCollection.Add(new Point(0, 0));
            myTextureCoordinatesCollection.Add(new Point(1, 1));
            myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

            // Create a collection of triangle indices for the MeshGeometry3D.
            Int32Collection myTriangleIndicesCollection = new Int32Collection();
            myTriangleIndicesCollection.Add(0);
            myTriangleIndicesCollection.Add(1);
            myTriangleIndicesCollection.Add(2);
            myTriangleIndicesCollection.Add(3);
            myTriangleIndicesCollection.Add(4);
            myTriangleIndicesCollection.Add(5);
            myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

            GeometryModel3D myGeometryModel = new GeometryModel3D();
            myGeometryModel.Geometry = myMeshGeometry3D;
            myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)));

            return myGeometryModel;
        }
        public static Viewport3D get3DViewport(ModelSkeletal modelSkel, ModelTexture? textureFile = null, ModelCollision? collisionFile = null)
        {
            Viewport3D myViewport3D = new Viewport3D();
            Model3DGroup myModel3DGroup = new Model3DGroup();
            ModelVisual3D myModelVisual3D = new ModelVisual3D();

            PerspectiveCamera myPCamera = new PerspectiveCamera();
            myPCamera.Position = new Point3D(0, 0, 500);
            myPCamera.LookDirection = new Vector3D(0, 0, -1);
            myPCamera.FieldOfView = 60;
            myViewport3D.Camera = myPCamera;

            AmbientLight myAmbientLight = new AmbientLight(System.Windows.Media.Brushes.White.Color);
            myModel3DGroup.Children.Add(myAmbientLight);

            foreach (ModelSkeletal.SkeletalGroup mesh in modelSkel.Groups)
            {
                myModel3DGroup.Children.Add(getGeometryFromGroup(mesh, textureFile));
            }

            //--------------------------------------------------------------------------------------
            // TEST
            if (collisionFile != null)
            {
                foreach (ObjectCollision collision in collisionFile.EntryList)
                {
                    if (collision.Bone == 16384 || collision.Bone == 2)
                        myModel3DGroup.Children.Add(getSquare(collision.Radius, new Vector3D(collision.PositionX, collision.PositionY, collision.PositionZ)));
                }
            }
            //myModel3DGroup.Children.Add(getSquare(90));
            //--------------------------------------------------------------------------------------

            // Add the group of models to the ModelVisual3d.
            myModelVisual3D.Content = myModel3DGroup;

            myViewport3D.Children.Add(myModelVisual3D);

            // Apply the viewport to the page so it will be rendered.
            return myViewport3D;
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
    }
}
