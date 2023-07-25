using System.IO;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class Main_Window : Window
    {
        public Main_ViewModel ThisVM { get; set; }

        public Main_Window()
        {
            InitializeComponent();

            ThisVM = new Main_ViewModel();

            DataContext = ThisVM;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();

                if (Directory.Exists(firstFile)){
                    ThisVM.FolderPath = firstFile;
                }
                bool isFile = File.Exists(firstFile);

                ObjectSelector_Control retr = new ObjectSelector_Control(ThisVM);

                ObjectSelector.Content = retr;
                Viewport.Content = new Viewport_Control(ThisVM);

                MotionSelector_Control msc = new MotionSelector_Control(ThisVM);

                MotionSelector.Content = msc;

                //loadFile(firstFile);
            }
        }
    }
}
