using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.UI
{
    public partial class M_UI_Control : UserControl
    {
        public M_UI_Control()
        {
            InitializeComponent();
            loadImages();
        }

        private void loadImages()
        {
            if(ApdxService.Instance.ImgdFace != null)
            {
                BitmapSource faceBitmap = ApdxService.Instance.ImgdFace.GetBimapSource();
                FaceFrame.Source = faceBitmap;
            }
            if (ApdxService.Instance.ImgdCommand != null)
            {
                BitmapSource commandBitmap = ApdxService.Instance.ImgdCommand.GetBimapSource();
                CommandFrame.Source = commandBitmap;
            }
        }

        private void Face_Export(object sender, System.Windows.RoutedEventArgs e)
        {
            exportImage(ApdxService.Instance.ImgdFace, "Face");
        }
        private void Face_Replace(object sender, System.Windows.RoutedEventArgs e)
        {
            Imgd loadedImgd = ImageUtils.loadPngFileAsImgd();
            ApdxService.Instance.ImgdFace = loadedImgd;
            loadImages();
        }

        private void Command_Export(object sender, System.Windows.RoutedEventArgs e)
        {
            exportImage(ApdxService.Instance.ImgdCommand, "Command");
        }
        private void Command_Replace(object sender, System.Windows.RoutedEventArgs e)
        {
            Imgd loadedImgd = ImageUtils.loadPngFileAsImgd();
            ApdxService.Instance.ImgdCommand = loadedImgd;
            loadImages();
        }

        private void exportImage(Imgd image, string imageName)
        {
            BitmapSource bitmapImage = image.GetBimapSource();

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export image as PNG";
            sfd.FileName = imageName + ".png";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                AssimpGeneric.ExportBitmapSourceAsPng(bitmapImage, sfd.FileName);
            }
        }
    }
}
