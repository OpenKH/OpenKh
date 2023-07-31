using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModelBoneViewer_Control : UserControl
    {
        ModelBoneViewer_VM ThisVM { get; set; }

        public ModelBoneViewer_Control()
        {
            InitializeComponent();
        }
        public ModelBoneViewer_Control(ModelSkeletal modelFile)
        {
            InitializeComponent();
            ThisVM = new ModelBoneViewer_VM(modelFile);
            DataContext = ThisVM;
        }
    }
}
