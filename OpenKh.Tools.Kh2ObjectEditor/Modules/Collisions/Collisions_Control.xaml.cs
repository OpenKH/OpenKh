using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public partial class Collisions_Control : UserControl
    {
        public M_Collisions_VM ThisVM { get; set; }
        public Collisions_Control()
        {
            InitializeComponent();
            ThisVM = new M_Collisions_VM();
            DataContext = ThisVM;
        }

        private void Collision_Copy(object sender, System.Windows.RoutedEventArgs e)
        {
            ObjectCollision collision = (ObjectCollision)DataTable.SelectedCells[0].Item;
            if (collision == null)
                return;

            ThisVM.copyCollision(collision);
        }
        private void CollisionGroup_Copy(object sender, System.Windows.RoutedEventArgs e)
        {
            ObjectCollision collision = (ObjectCollision)DataTable.SelectedCells[0].Item;
            if (collision == null)
                return;

            ThisVM.copyCollisionGroup(collision.Group);
        }

        private void Collision_Replace(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.replaceCollision(DataTable.SelectedIndex);
        }
        private void Collision_AddGroup(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.addCopiedGroup();
        }

        private void Collision_Add(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.addCollision();
        }
        private void Collision_Remove(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.removeCollision(DataTable.SelectedIndex);
        }

        private void Button_Save(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.saveCollisions();
        }

        private void Button_Test(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.TestCollisionsIngame();
        }

        private void DataTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<int> selectedIndices = new List<int>();
            foreach (var selectedItem in DataTable.SelectedItems)
            {
                int index = DataTable.Items.IndexOf(selectedItem);
                if (index >= 0)
                {
                    selectedIndices.Add(index);
                }
            }
            ViewerService.Instance.ShowCollisions(selectedIndices);
        }
    }
}
