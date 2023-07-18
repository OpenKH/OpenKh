using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using Simple3DViewport.Controls;
using Simple3DViewport.Objects;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class CollisionV_Control : UserControl
    {
        Simple3DViewport_Control thisViewport { get; set; }
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

            if(CollisionVM.ThisModel != null)
            {
                thisViewport = new Simple3DViewport_Control(new List<SimpleModel> { CollisionVM.ThisModel });
            }
            else
            {
                thisViewport = new Simple3DViewport_Control();
            }
            
            thisViewport.setOpacityById(0.7, "MODEL_1");
            thisViewport.VPModels.Add(CollisionVM.ThisCollisions);
            thisViewport.setVisibilityByLabel(false, "COLLISION_SINGLE");

            viewportFrame.Content = thisViewport;
        }
        public void CollisionList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollisionWrapper collision = ((ListViewItem)sender).Content as CollisionWrapper;
            collisionPropertiesFrame.Content = new CollisionProperties_Control(collision.Collision);
        }
        public void CollisionList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollisionWrapper collision = ((ListViewItem)sender).Content as CollisionWrapper;
            collisionPropertiesFrame.Content = new CollisionProperties_Control(collision.Collision);
            collision.Selected_VM = !collision.Selected_VM;
            thisViewport.setVisibilityById(collision.Selected_VM, collision.Name);
            thisViewport.render();
        }
    }
}
