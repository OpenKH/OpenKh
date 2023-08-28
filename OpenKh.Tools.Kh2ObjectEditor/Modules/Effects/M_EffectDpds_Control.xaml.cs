using System.Windows;
using System.Windows.Controls;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Effects.M_EffectDpds_VM;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectDpds_Control : UserControl
    {
        public M_EffectDpds_VM ThisVM { get; set; }
        public M_EffectDpds_Control()
        {
            InitializeComponent();
            ThisVM = new M_EffectDpds_VM();
            DataContext = ThisVM;
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DpdWrapper item = (DpdWrapper)(sender as ListView).SelectedItem;

            DpdFrame.Content = new M_EffectsDpdTabs_Control(item.DpdItem);
        }
        public void Dpd_Copy(object sender, RoutedEventArgs e)
        {
            if (List_Dpds.SelectedIndex != null)
            {
                ThisVM.Dpd_Copy(List_Dpds.SelectedIndex);
            }
        }
        public void Dpd_AddCopied(object sender, RoutedEventArgs e)
        {
            ThisVM.Dpd_AddCopied();
        }
        public void Dpd_Replace(object sender, RoutedEventArgs e)
        {
            if (List_Dpds.SelectedIndex != null)
            {
                ThisVM.Dpd_Replace(List_Dpds.SelectedIndex);
            }
        }
    }
}
