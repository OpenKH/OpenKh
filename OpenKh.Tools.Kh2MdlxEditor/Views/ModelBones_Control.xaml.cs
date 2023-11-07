using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class ModelBones_Control : UserControl
    {
        ModelBones_VM ThisVM { get; set; }

        public ModelBones_Control()
        {
            InitializeComponent();
        }
        public ModelBones_Control(ModelSkeletal modelFile)
        {
            InitializeComponent();
            ThisVM = new ModelBones_VM(modelFile);
            DataContext = ThisVM;
        }
    }
}
