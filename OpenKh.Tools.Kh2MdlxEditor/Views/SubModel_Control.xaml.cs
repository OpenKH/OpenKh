using OpenKh.Kh2;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    /// <summary>
    /// Interaction logic for SubModelControl.xaml
    /// </summary>
    public partial class SubModel_Control : UserControl
    {
        public SubModel_Control()
        {
            InitializeComponent();
        }
        public SubModel_Control(Mdlx.SubModel subModel)
        {
            InitializeComponent();
            DataContext = subModel;
        }
    }
}
