using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static OpenKh.Kh2.Models.ModelCommon;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class CollisionV_VM
    {
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
                Collisions.Add(new CollisionWrapper("Collision " + i, collision));
            }
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
                //basePosition = boneMatrices[collision.Bone].Translation;
                collisionBox = Viewport3DUtils.getCube(collision.Radius, collision.Height, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), Color.FromArgb(100, 255, 0, 0));
            }
            else
            {
                collisionBox = Viewport3DUtils.getCube(collision.Radius, collision.Height, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), Color.FromArgb(100, 200, 200, 0));
            }

            return collisionBox;
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
