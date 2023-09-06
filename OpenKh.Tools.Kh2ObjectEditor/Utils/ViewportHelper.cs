using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Common.Wpf;
using Simple3DViewport.Objects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenKh.Tools.Kh2ObjectEditor.Utils
{
    public class ViewportHelper
    {
        public static SimpleModel getModel(ModelSkeletal modelFile, ModelTexture textureFile)
        {
            if (modelFile != null)
            {
                List<GeometryModel3D> meshes = getGeometryFromModel(modelFile, textureFile);
                List<SimpleMesh> simpleMeshes = new List<SimpleMesh>();
                for (int i = 0; i < meshes.Count; i++)
                {
                    simpleMeshes.Add(new SimpleMesh(meshes[i], "MESH_" + i, new List<string> { "MODEL" }));
                }
                return new SimpleModel(simpleMeshes, "MODEL_1", new List<string> { "MODEL" });
            }
            return null;
        }
        /*public static SimpleModel getCollisions(ModelSkeletal modelFile, ModelCollision collisionFile)
        {
            if (collisionFile != null)
            {
                Matrix4x4[] boneMatrices = new Matrix4x4[0];

                if (modelFile != null)
                    boneMatrices = GetBoneMatrices(modelFile.Bones);

                List<SimpleMesh> simpleMeshes = new List<SimpleMesh>();

                for (int i = 0; i < Collisions.Count; i++)
                {
                    ObjectCollision collision = Collisions[i].Collision;

                    Vector3 basePosition = Vector3.Zero;
                    if (collision.Bone != 16384 && boneMatrices.Length != 0)
                    {
                        basePosition = Vector3.Transform(new Vector3(collision.PositionX, collision.PositionY, collision.PositionZ), boneMatrices[collision.Bone]);
                    }


                    Color color = new Color();
                    if (collision.Type == (byte)ObjectCollision.TypeEnum.HIT)
                    {
                        color = Color.FromArgb(100, 255, 0, 0);
                    }
                    else if (collision.Type == (byte)ObjectCollision.TypeEnum.REACTION)
                    {
                        color = Color.FromArgb(100, 0, 255, 0);
                    }
                    else
                    {
                        color = Color.FromArgb(100, 0, 0, 255);
                    }

                    if (collision.Shape == (byte)ObjectCollision.ShapeEnum.ELLIPSOID)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getEllipsoid(collision.Radius, collision.Height, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == (byte)ObjectCollision.ShapeEnum.COLUMN)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getCylinder(collision.Radius, collision.Height, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == (byte)ObjectCollision.ShapeEnum.CUBE)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getCuboid(collision.Radius, collision.Height, collision.Radius, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == (byte)ObjectCollision.ShapeEnum.SPHERE)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getSphere(collision.Radius, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                }

                return new SimpleModel(simpleMeshes, "COLLISIONS_1", new List<string> { "COLLISION", "COLLISION_GROUP" });
            }
            return null;
        }*/

        // FROM MDLXEDITOR
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
                    myMaterial = Simple3DViewport.Utils.Simple3DUtils.getDefaultMaterial();
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
                myMaterial = Simple3DViewport.Utils.Simple3DUtils.getDefaultMaterial();
            }

            myGeometryModel.Material = myMaterial;

            return myGeometryModel;
        }

        // FROM MDLXEDITOR
        public static List<GeometryModel3D> getGeometryFromModel(ModelSkeletal modelFile, ModelTexture? textureFile = null)
        {
            List<GeometryModel3D> geometryList = new List<GeometryModel3D>();
            foreach (ModelSkeletal.SkeletalGroup group in modelFile.Groups)
            {
                geometryList.Add(getGeometryFromGroup(group, textureFile));
            }

            return geometryList;
        }
    }
}
