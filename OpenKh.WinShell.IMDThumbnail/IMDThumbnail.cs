using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;

using OpenKh.Kh2;
using OpenKh.Imaging;

namespace OpenKh.WinShell.IMDUtilities
{
    [Obsolete]
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".imd")]
    public class IMDThumbnail : SharpThumbnailHandler
    {
        Lazy<IImageRead> _tImage;

        protected override Bitmap GetThumbnailImage(uint width)
        {
            using (MemoryStream _cStream = new MemoryStream())
            {
                SelectedItemStream.CopyTo(_cStream);

                _tImage = new Lazy<IImageRead>(() => Imgd.Read(_cStream));

                var size = _tImage.Value.Size;
                var data = _tImage.Value.ToBgra32();

                MarshalBitmap _tBitmap = new MarshalBitmap(size.Width, size.Height, data);

                return _tBitmap.Bitmap;
            }
        }
    }
}
