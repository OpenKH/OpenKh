using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using Simple3DViewport.Objects;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static OpenKh.Kh2.Models.ModelCommon;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class CollisionV_VM
    {
        public SimpleModel ThisModel { get; set; }
        public SimpleModel ThisCollisions { get; set; }
        public ModelSkeletal? ModelFile { get; set; }
        public ModelTexture? TextureFile { get; set; }
        public ModelCollision CollisionFile { get; set; }
        public List<CollisionWrapper> Collisions { get; set; }

        public CollisionV_VM() { }
        public CollisionV_VM(ModelCollision collisionFile, ModelSkeletal? modelFile = null, ModelTexture? textureFile = null)
        {
            this.ModelFile = modelFile;
            this.TextureFile = textureFile;
            this.CollisionFile = collisionFile;

            Collisions = new List<CollisionWrapper>();
            for (int i = 0; i < this.CollisionFile.EntryList.Count; i++)
            {
                ObjectCollision collision = this.CollisionFile.EntryList[i];
                Collisions.Add(new CollisionWrapper("COLLISION_" + i, collision));
            }

            loadModel();
            loadCollisions();
        }
        public GeometryModel3D getCollisionBox(ObjectCollision collision)
        {
            GeometryModel3D collisionBox = new GeometryModel3D();
            Matrix4x4[] boneMatrices = new Matrix4x4[0];
            if(ModelFile != null) boneMatrices = GetBoneMatrices(ModelFile.Bones);

            Vector3 basePosition = Vector3.Zero;
            if (collision.Bone != 16384 && boneMatrices.Length != 0)
            {
                basePosition = Vector3.Transform(new Vector3(collision.PositionX, collision.PositionY, collision.PositionZ), boneMatrices[collision.Bone]);
                collisionBox = OpenKh.Tools.Kh2MdlxEditor.Utils.Viewport3DUtils.getCube(collision.Radius, collision.Height, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), Color.FromArgb(100, 255, 0, 0));
            }
            else
            {
                collisionBox = OpenKh.Tools.Kh2MdlxEditor.Utils.Viewport3DUtils.getCube(collision.Radius, collision.Height, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), Color.FromArgb(100, 200, 200, 0));
            }

            return collisionBox;
        }

        public void loadModel()
        {
            if (ModelFile != null)
            {
                List<GeometryModel3D> meshes = OpenKh.Tools.Kh2MdlxEditor.Utils.Viewport3DUtils.getGeometryFromModel(ModelFile, TextureFile);
                List<SimpleMesh> simpleMeshes = new List<SimpleMesh>();
                for (int i = 0; i < meshes.Count; i++)
                {
                    simpleMeshes.Add(new SimpleMesh(meshes[i], "MESH_"+i, new List<string> { "MODEL" }));
                }
                ThisModel = new SimpleModel(simpleMeshes, "MODEL_1", new List<string> { "MODEL" });
            }
        }
        public void loadCollisions()
        {
            if (CollisionFile != null)
            {
                Matrix4x4[] boneMatrices = new Matrix4x4[0];

                if (ModelFile != null) boneMatrices = GetBoneMatrices(ModelFile.Bones);

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
                    if (collision.Type == ObjectCollision.TypeEnum.HIT)
                    {
                        color = Color.FromArgb(100, 255, 0, 0);
                    }
                    else if (collision.Type == ObjectCollision.TypeEnum.REACTION)
                    {
                        color = Color.FromArgb(100, 0, 255, 0);
                    }
                    else
                    {
                        color = Color.FromArgb(100, 0, 0, 255);
                    }

                    if(collision.Shape == ObjectCollision.ShapeEnum.ELLIPSOID)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getEllipsoid(collision.Radius, collision.Height, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == ObjectCollision.ShapeEnum.COLUMN)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getCylinder(collision.Radius, collision.Height, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == ObjectCollision.ShapeEnum.CUBE)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getCuboid(collision.Radius, collision.Height, collision.Radius, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == ObjectCollision.ShapeEnum.SPHERE)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getSphere(collision.Radius, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                }

                ThisCollisions = new SimpleModel(simpleMeshes, "COLLISIONS_1", new List<string> { "COLLISION", "COLLISION_GROUP" });
            }
        }
    }


    public class CollisionWrapper : INotifyPropertyChanged
    {
        public bool selected { get; set; }
        public bool Selected_VM { get => selected; set { selected = value; NotifyPropertyChanged("Selected_VM"); } }
        public string Name { get; set; }
        public ObjectCollision Collision { get; set; }

        public CollisionWrapper(string name, ObjectCollision collision)
        {
            this.Name = name;
            this.Collision = collision;
            this.Selected_VM = false;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
