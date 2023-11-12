using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Modules.Model;
using System.Windows.Controls;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Model.ModuleModel_VM;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModuleModel_Control : UserControl
    {
        public ModuleModel_VM ThisVM { get; set; }
        public ModuleModel_Control()
        {
            InitializeComponent();
            ThisVM = new ModuleModel_VM();
            DataContext = ThisVM;
        }
    }
}
