using OpenKh.Tools.Kh2ObjectEditor.Utils;
using Simple3DViewport.Objects;
using Simple3DViewport.Controls;
using System;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public class Viewport_ViewModel
    {
        public Main_ViewModel MainVM { get; set; }
        public SimpleModel ThisModel { get; set; }
        public SimpleModel ThisCollisions { get; set; }

        public Simple3DViewport_Control ViewportControl { get; set; }

        public Viewport_ViewModel()
        {
            MainVM = new Main_ViewModel();
            Subscribe();
        }

        public Viewport_ViewModel(Main_ViewModel mainVM, Simple3DViewport_Control viewportControl)
        {
            MainVM = mainVM;
            ViewportControl = viewportControl;
            loadModel();
            Subscribe();
        }

        public void loadModel()
        {
            if (MainVM.LoadedObject != null)
            {
                ThisModel = ViewportHelper.getModel(MainVM.LoadedObject.ModelFile, MainVM.LoadedObject.TextureFile);
                ViewportControl.VPModels.Clear();
                if(ThisModel != null) ViewportControl.VPModels.Add(ThisModel);
                ViewportControl.render();
                ViewportControl.restartCamera();
            }
        }

        // Event load file
        public void Subscribe()
        {
            MainVM.Load += new Main_ViewModel.EventHandler(MyFunction);
        }
        private void MyFunction(Main_ViewModel m, EventArgs e)
        {
            loadModel();
        }
    }
}
