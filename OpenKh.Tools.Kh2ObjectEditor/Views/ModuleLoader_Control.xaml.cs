using OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions;
using OpenKh.Tools.Kh2ObjectEditor.Modules.Effects;
using OpenKh.Tools.Kh2ObjectEditor.Modules.Textures;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModuleLoader_Control : UserControl
    {
        public ModuleLoader_VM ThisVM { get; set; }
        public ModuleLoader_Control()
        {
            InitializeComponent();
            ThisVM = new ModuleLoader_VM();
            DataContext = ThisVM;
            subscribe_ObjectSelected();
            reloadTabs();
        }

        public void reloadTabs()
        {
            TabModel.Visibility = ThisVM.TabModelEnabled;
            TabTextures.Visibility = ThisVM.TabTexturesEnabled;
            TabCollisions.Visibility = ThisVM.TabCollisionsEnabled;
            TabMotions.Visibility = ThisVM.TabMotionsEnabled;
            TabParticles.Visibility = ThisVM.TabParticlesEnabled;
            TabAI.Visibility = ThisVM.TabAIEnabled;
        }


        public void subscribe_ObjectSelected()
        {
            App_Context.Instance.Event_ObjectSelected += new App_Context.EventHandler(MyFunction);
        }
        private void MyFunction(App_Context m, EventArgs e)
        {
            contentFrame.Content = new ContentControl();
            reloadTabs();
        }

        private void TabClick_Model(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            contentFrame.Content = new ModuleModel_Control();
        }
        private void TabClick_Textures(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            contentFrame.Content = new ModuleTextures_Control();
            //contentFrame.Content = new TextureAnimation_Control();
        }
        private void TabClick_Collisions(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            contentFrame.Content = new Collisions_Control();
        }
        private void TabClick_Motions(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            contentFrame.Content = new ModuleMotions_Control();
        }
        private void TabClick_Particles(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //contentFrame.Content = new EffectTest_Control();
            contentFrame.Content = new M_Effects_Control();
        }
        private void TabClick_AI(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            contentFrame.Content = new ModuleAI_Control();
        }
    }
}
