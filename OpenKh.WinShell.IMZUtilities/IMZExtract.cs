using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

using OpenKh.Kh2;
using OpenKh.Imaging;

namespace OpenKh.WinShell.IMZUtilities
{
    [Obsolete]
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".imz")]
    public class IMZExtract : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var itemExtract = new ToolStripMenuItem { Text = "Extract IMGZ..." };
            itemExtract.Click += (sender, args) => ExtractFunction();

            menu.Items.Add(itemExtract);
            return menu;
        }

        private void ExtractFunction()
        {
            foreach (var filePath in SelectedItemPaths)
            {
                using (FileStream _cStream = new FileStream(filePath, FileMode.Open))
                {
                    string _fPath = filePath.Replace(".imz", "");
                    Imgz _tArchive = new Imgz(_cStream);

                    Directory.CreateDirectory(_fPath);
                    int _i = 0;

                    foreach (var _tImage in _tArchive.Images)
                    {
                        using (FileStream _tStream = new FileStream(_fPath + string.Format("\\{0}.imd", _i.ToString("000")), FileMode.OpenOrCreate))
                        {
                            _tImage.Write(_tStream);
                            _tStream.Close();
                        }

                        _i++;
                    }

                    _cStream.Close();
                }
            }
        }
    }
}
