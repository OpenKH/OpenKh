using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class CollisionV_Control : UserControl
    {
        CollisionV_VM CollisionVM { get; set; }
        public CollisionV_Control()
        {
            InitializeComponent();
        }
        public CollisionV_Control(ModelCollision collisionFile, ModelSkeletal? modelFile = null, ModelTexture? textureFile = null)
        {
            InitializeComponent();
            CollisionVM = new CollisionV_VM(collisionFile, modelFile, textureFile);
            DataContext = CollisionVM;

            drawSelectedGroups();
        }
        public void CollisionList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollisionWrapper collision = ((ListViewItem)sender).Content as CollisionWrapper;
            //meshPropertiesFrame.Content = new MeshProperties_Control(collision.Collision);
        }
        public void CollisionList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollisionWrapper collision = ((ListViewItem)sender).Content as CollisionWrapper;
            //meshPropertiesFrame.Content = new MeshProperties_Control(collision.Collision);
            collision.Selected_VM = !collision.Selected_VM;
            drawSelectedGroups();
        }

        public void drawSelectedGroups()
        {
            List<GeometryModel3D> geometry = new List<GeometryModel3D>();

            // Model
            if (CollisionVM.ModelFile != null) {
                geometry.AddRange(Viewport3DUtils.getGeometryFromModel(CollisionVM.ModelFile, CollisionVM.TextureFile));
            }

            // Collisions
            foreach (CollisionWrapper collision in CollisionVM.Collisions)
            {
                if (collision.Selected_VM) {
                    geometry.Add(CollisionVM.getCollisionBox(collision.Collision));
                }
            }

            if(viewportFrame.Content == null)
            {
                viewportFrame.Content = new Viewport_Control(geometry);
            }
            else
            {
                viewportFrame.Content = new Viewport_Control(geometry, ((Viewport_Control)viewportFrame.Content).VPCamera);
            }
            
        }
    }
}
