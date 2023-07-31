using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    /*
     * Contains the controls that display modifiable data
     */
    public partial class DataViewer_Control : UserControl
    {
        public DataViewer_Control()
        {
            InitializeComponent();
            subscribe_MotionSelected();
        }

        public void openMotionTriggers()
        {
            LoadControl.Content = new MotionTriggers_Control(App_Context.Instance.AnimBinary);
            //LoadControl.Content = new ModelBoneViewer_Control(App_Context.Instance.ModelFile);
        }

        public void subscribe_MotionSelected()
        {
            App_Context.Instance.Event_MotionSelected += new App_Context.EventHandler(sub_MotionSelected);
        }
        private void sub_MotionSelected(App_Context m, EventArgs e)
        {
            openMotionTriggers();
        }
    }
}
