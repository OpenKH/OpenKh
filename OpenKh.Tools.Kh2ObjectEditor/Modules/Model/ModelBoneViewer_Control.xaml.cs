using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2ObjectEditor.Modules.Model;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModelBoneViewer_Control : UserControl
    {
        ModelBoneViewer_VM ThisVM { get; set; }

        public ModelBoneViewer_Control()
        {
            InitializeComponent();
            if(MdlxService.Instance.ModelFile != null)
            {
                ThisVM = new ModelBoneViewer_VM(MdlxService.Instance.ModelFile);
                DataContext = ThisVM;
            }
        }
        public ModelBoneViewer_Control(ModelSkeletal modelFile)
        {
            InitializeComponent();
            ThisVM = new ModelBoneViewer_VM(modelFile);
            DataContext = ThisVM;
        }
    }
}
