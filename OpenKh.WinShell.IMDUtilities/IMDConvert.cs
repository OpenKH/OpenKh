using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

using OpenKh.Kh2;
using OpenKh.Imaging;

namespace OpenKh.WinShell.IMDUtilities
{
    [Obsolete]
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".imd")]
    public class IMDConvert : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var itemConvert = new ToolStripMenuItem
            {
                Text = "Convert to PNG..."
            };

            itemConvert.Click += (sender, args) => ConvertPNG();

            menu.Items.Add(itemConvert);
            return menu;
        }

        private void ConvertPNG()
        {
            foreach (var filePath in SelectedItemPaths)
            {
                using (FileStream _cStream = new FileStream(filePath, FileMode.Open))
                {
                    Imgd _tImage = Imgd.Read(_cStream);

                    var size = _tImage.Size;
                    var data = _tImage.ToBgra32();

                    MarshalBitmap _tBitmap = new MarshalBitmap(size.Width, size.Height, data);
                    _tBitmap.Bitmap.Save(filePath.Replace(".imd", ".png"));

                    _tBitmap.Dispose();
                }
            }
        }
    }
}
