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
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MeshWrapper item = (MeshWrapper)(sender as ListView).SelectedItem;

            MeshFrame.Content = new ModuleModelMesh_Control(item.Group);
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
    }
}
