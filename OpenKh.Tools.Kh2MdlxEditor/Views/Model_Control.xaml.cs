using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class Model_Control : UserControl
    {
        Model_VM modelControlModel { get; set; }

        public Model_Control()
        {
            InitializeComponent();
        }
        public Model_Control(ModelSkeletal modelFile, ModelTexture textureFile, ModelCollision? collisionFile = null)
        {
            InitializeComponent();

            // Recalc meshes
            modelFile.recalculateMeshes();

            modelControlModel = new Model_VM(modelFile, textureFile, collisionFile);
            DataContext = modelControlModel;
            List<GeometryModel3D> geometry = new List<GeometryModel3D>();
            geometry.AddRange(Viewport3DUtils.getGeometryFromModel(modelControlModel.ModelFile, modelControlModel.TextureFile));
            //if(modelControlModel.CollisionFile != null)
            //geometry.AddRange(modelControlModel.getCollisionBoxes());
            //geometry.AddRange(modelControlModel.getBoneBoxes());
            viewportFrame.Content = new Viewport_Control(geometry);
        }

        // Loads the required control in the frame on BAR item click
        public void MeshList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GroupWrapper group = ((ListViewItem)sender).Content as GroupWrapper;
            meshPropertiesFrame.Content = new MeshProperties_Control(group.Group);
        }
        public void MeshList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            GroupWrapper group = ((ListViewItem)sender).Content as GroupWrapper;
            meshPropertiesFrame.Content = new MeshProperties_Control(group.Group);
            group.Selected_VM = !group.Selected_VM;
            drawSelectedGroups();
        }
        public void drawSelectedGroups()
        {
            List<GeometryModel3D> geometry = new List<GeometryModel3D>();
            foreach (GroupWrapper group in modelControlModel.Groups)
            {
                if (group.Selected_VM)
                {
                    geometry.Add(Viewport3DUtils.getGeometryFromGroup(group.Group, modelControlModel.TextureFile));
                }
            }
            viewportFrame.Content = new Viewport_Control(geometry, ((Viewport_Control)viewportFrame.Content).VPCamera);
        }
    }
}
