using OpenKh.Tools.Kh2ObjectEditor.Modules.Model;
using System.Windows.Controls;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Model.ModuleModelMeshes_VM;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModuleModelMeshes_Control : UserControl
    {
        public ModuleModelMeshes_VM ThisVM { get; set; }
        public ModuleModelMeshes_Control()
        {
            InitializeComponent();
            ThisVM = new ModuleModelMeshes_VM();
            DataContext = ThisVM;
            if(ThisVM.Meshes.Count > 0)
            {
                List_Meshes.SelectedIndex = 0;
                MeshFrame.Content = new ModuleModelMesh_Control(ThisVM.Meshes[0].Group);
            }
        }

        private void Button_MoveMeshUp(object sender, System.Windows.RoutedEventArgs e)
        {
            if (List_Meshes.SelectedItem == null)
                return;

            int newIndex = ThisVM.fun_moveMeshUp((MeshWrapper)List_Meshes.SelectedItem);

            if (newIndex != -1)
            {
                List_Meshes.SelectedIndex = newIndex;
            }
        }
        private void Button_MoveMeshDown(object sender, System.Windows.RoutedEventArgs e)
        {
            if (List_Meshes.SelectedItem == null)
                return;

            int newIndex = ThisVM.fun_moveMeshDown((MeshWrapper)List_Meshes.SelectedItem);

            if (newIndex != -1)
            {
                List_Meshes.SelectedIndex = newIndex;
            }
        }

        private void Meshes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((MeshWrapper)(sender as ListView).SelectedItem == null) {
                return;
            }
            MeshWrapper item = (MeshWrapper)(sender as ListView).SelectedItem;

            MeshFrame.Content = new ModuleModelMesh_Control(item.Group);
        }
    }
}
