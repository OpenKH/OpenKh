using OpenKh.Kh2;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace OpenKh.Tools.Kh2MsetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void loadFile(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                //handler.dropFile(files[0]);
                Debug.WriteLine("MY FILE: " + files[0]);
                //OpenBarFile(files[0]);
                OpenMdlxFile(files[0]);
                //Debug.WriteLine("MY FILE: ");

                //File loadedFile = 
            }
        }

        public void OpenBarFile(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open);
            Bar binarc = Bar.Read(stream);
            if (binarc != null)
                Debug.WriteLine("BAR: " + binarc[0].Name);
        }
        public void OpenMdlxFile(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open);
            Bar binarc = Bar.Read(stream);
            if (binarc != null)
            {
                Mdlx model = Mdlx.Read(binarc[0].Stream);
                ModelTexture textureData = ModelTexture.Read(binarc[1].Stream);
                Debug.WriteLine("MDLX: " + model);
            }
        }
    }
}
