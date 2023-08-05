using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class Model_Control : UserControl
    {
        Main2_VM mainVM { get; set; }
        Model_VM modelControlModel { get; set; }

        public Model_Control()
        {
            InitializeComponent();
        }
        public Model_Control(Main2_VM mainVm)
        {
            this.mainVM = mainVm;

            InitializeComponent();
            modelControlModel = new Model_VM(mainVm.ModelFile, mainVm.TextureFile, mainVm.CollisionFile);
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

        public void Mesh_MoveUp(object sender, RoutedEventArgs e)
        {
            if (LV_Meshes.SelectedItem == null) return;

            MoveCurrentMeshBy(-1);
        }
        
        public void Mesh_MoveDown(object sender, RoutedEventArgs e)
        {
            if (LV_Meshes.SelectedItem == null) return;

            MoveCurrentMeshBy(1);
        }

        private void MoveCurrentMeshBy(int offset)
        {
            // calculate index with a wraparound values
            var swapIndex = ((LV_Meshes.SelectedIndex + offset + LV_Meshes.Items.Count) % LV_Meshes.Items.Count); ;
            SwapGroups(LV_Meshes.SelectedIndex, swapIndex);
            
            mainVM.reloadModelFromFbx();
            
        }

        private void SwapGroups(int index, int swapIndex)
        {
            var viewModelGroups = modelControlModel.Groups;
            (viewModelGroups[index], viewModelGroups[swapIndex])  = (viewModelGroups[swapIndex], viewModelGroups[index]);

            var modelSkeletal = mainVM.ModelFile;
            var skeletalGroups = modelSkeletal.Groups;
            (skeletalGroups[index], skeletalGroups[swapIndex]) = (skeletalGroups[swapIndex], skeletalGroups[index]);

            // var shadowGroups = modelSkeletal.Shadow.Groups;
            // (shadowGroups[index], shadowGroups[swapIndex])  = (shadowGroups[swapIndex], shadowGroups[index]);
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
