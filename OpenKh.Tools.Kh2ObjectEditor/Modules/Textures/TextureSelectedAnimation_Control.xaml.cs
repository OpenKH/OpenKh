using ModelingToolkit.Objects;
using System.Windows.Controls;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Textures.TextureSelectedAnimation_VM;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public partial class TextureSelectedAnimation_Control : UserControl
    {
        TextureSelectedAnimation_VM ThisVM { get; set; }
        public TextureSelectedAnimation_Control(int index)
        {
            InitializeComponent();
            ThisVM = new TextureSelectedAnimation_VM(index);
            DataContext = ThisVM;
            loadImage(index);
        }

        public void loadImage(int index)
        {
            MtMaterial mat = ThisVM.GetMaterial(index);
            ImageFrame.Source = mat.GetAsBitmapImage();
            TexAnimCanvas.Height = mat.Height;
        }

        private void Script_Export(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ScriptList.SelectedItem == null)
                return;

            ScriptWrapper item = (ScriptWrapper)ScriptList.SelectedItem;
            ThisVM.Script_Export(item.Id);
        }
        private void Script_Import(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.Script_Import();
        }
        private void Script_Replace(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ScriptList.SelectedItem == null)
                return;

            ScriptWrapper item = (ScriptWrapper)ScriptList.SelectedItem;
            ThisVM.Script_Replace(item.Id);
        }
        private void Script_Remove(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ScriptList.SelectedItem == null)
                return;

            ScriptWrapper item = (ScriptWrapper)ScriptList.SelectedItem;
            ThisVM.Script_Remove(item.Id);
        }
    }
}
