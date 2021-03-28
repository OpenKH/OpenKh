using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

using OpenKh.Kh2;
using OpenKh.Imaging;
using System.Collections.Generic;

namespace OpenKh.WinShell.IMZUtilities
{
    [Obsolete]
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".imd")]
    public class IMZRepack : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var itemPack = new ToolStripMenuItem { Text = "Pack into IMGZ..." };
            itemPack.Click += (sender, args) => RepackFunction();

            menu.Items.Add(itemPack);
            return menu;
        }

        private void RepackFunction()
        {
            List<Imgd> _tList = new List<Imgd>();
            string _rPath = "";

            foreach (var filePath in SelectedItemPaths)
            {
                if (_rPath == "")
                    _rPath = Path.GetDirectoryName(filePath);

                using (FileStream _cStream = new FileStream(filePath, FileMode.Open))
                {
                    Imgd _tImage = Imgd.Read(_cStream);
                    _tList.Add(_tImage);
                    _cStream.Close();
                }
            }

            using (FileStream _oStream = new FileStream(_rPath + "\\output.imz", FileMode.OpenOrCreate))
            {
                Imgz.Write(_oStream, _tList.ToArray());
                _oStream.Close();
            }
        }
    }
}
