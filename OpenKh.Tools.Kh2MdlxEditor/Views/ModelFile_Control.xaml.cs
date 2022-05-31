using OpenKh.Kh2;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using static OpenKh.Tools.Kh2MdlxEditor.ViewModels.ModelFile_VM;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    /// <summary>
    /// Interaction logic for ModelFileControl.xaml
    /// </summary>
    public partial class ModelFile_Control : UserControl
    {
        ModelFile_VM modelControlModel { get; set; }
        public ModelFile_Control()
        {
            InitializeComponent();
            modelControlModel = new ModelFile_VM();
            DataContext = modelControlModel;
        }
        public ModelFile_Control(Mdlx ModelFile)
        {
            InitializeComponent();
            modelControlModel = new ModelFile_VM(ModelFile);
            DataContext = modelControlModel;

            if (modelControlModel.ModelFile != null && modelControlModel.ModelFile.SubModels.Count > 0)
                contentFrame.Content = new SubModel_Control(modelControlModel.ModelFile.SubModels[0]);
        }

        public void ListViewItem_OpenSubModel(object sender, MouseButtonEventArgs e)
        {
            contentFrame.Content = new SubModel_Control((((ListViewItem)sender).Content as SubModelWrapper).subModel);
        }
    }
}
