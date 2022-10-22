using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static OpenKh.Kh2.Models.ModelCommon;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    public class Model_VM
    {
        public ModelSkeletal ModelFile { get; set; }
        public ModelTexture? TextureFile { get; set; }
        public ModelCollision? CollisionFile { get; set; }
        public List<GroupWrapper> Groups { get; set; }

        public Model_VM() { }
        public Model_VM(ModelSkeletal modelFile, ModelTexture? textureFile = null, ModelCollision? collisionFile = null)
        {
            this.ModelFile = modelFile;
            this.TextureFile = textureFile;
            this.CollisionFile = collisionFile;

            Groups = new List<GroupWrapper>();
            for (int i = 0; i < this.ModelFile.Groups.Count; i++)
            {
                ModelSkeletal.SkeletalGroup group = this.ModelFile.Groups[i];
                Groups.Add(new GroupWrapper("Mesh" + i + " [" + group.Header.PolygonCount + " tris]", group));
            }
        }

        public List<GeometryModel3D> getCollisionBoxes()
        {
            List<GeometryModel3D> collisionBoxes = new List<GeometryModel3D>();
            Matrix4x4[] boneMatrices = GetBoneMatrices(ModelFile.Bones);

            foreach (ObjectCollision collision in CollisionFile.EntryList)
            {
                Vector3 basePosition = Vector3.Zero;
                if (collision.Bone != 16384)
                {
                    basePosition = Vector3.Transform(new Vector3(collision.PositionX, collision.PositionY, collision.PositionZ), boneMatrices[collision.Bone]);
                    //basePosition = boneMatrices[collision.Bone].Translation;
                    collisionBoxes.Add(Viewport3DUtils.getCube(collision.Radius, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), Color.FromArgb(100, 255, 0, 0)));
                }
                else
                {
                    collisionBoxes.Add(Viewport3DUtils.getCube(collision.Radius, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), Color.FromArgb(100, 200, 200, 0)));
                }

                //collisionBoxes.Add(Viewport3DUtils.getCube(collision.Radius, new Vector3D(basePosition.X + collision.PositionX, basePosition.Y + collision.PositionY, basePosition.Z + collision.PositionZ)));
                
            }

            return collisionBoxes;
        }
        public List<GeometryModel3D> getBoneBoxes()
        {
            List<GeometryModel3D> boneBoxes = new List<GeometryModel3D>();
            Matrix4x4[] boneMatrices = GetBoneMatrices(ModelFile.Bones);

            for (int i = 0; i < ModelFile.Bones.Count; i++)
            {
                Vector3 basePosition = boneMatrices[i].Translation;

                boneBoxes.Add(Viewport3DUtils.getCube(10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), Color.FromArgb(100, 0, 255, 0)));
            }

            return boneBoxes;
        }
    }

    public class GroupWrapper : INotifyPropertyChanged
    {
        public bool selected { get; set; }
        public bool Selected_VM { get => selected; set { selected = value; NotifyPropertyChanged("Selected_VM"); } }
        public string Name { get; set; }
        public int triCount { get; set; }
        public ModelSkeletal.SkeletalGroup Group { get; set; }

        public GroupWrapper(string name, ModelSkeletal.SkeletalGroup group)
        {
            this.Name = name;
            this.Group = group;
            this.Selected_VM = true;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
