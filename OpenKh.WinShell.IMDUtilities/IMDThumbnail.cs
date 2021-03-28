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
        Lazy<Bitmap> _rBitmap;
        protected override Bitmap GetThumbnailImage(uint width)
        {
            using (MemoryStream _cStream = new MemoryStream())
            {
                SelectedItemStream.CopyTo(_cStream);
                SelectedItemStream.Dispose();

                _tImage = new Lazy<IImageRead>(() => Imgd.Read(_cStream));

                var size = _tImage.Value.Size;
                var data = _tImage.Value.ToBgra32();

                using (MarshalBitmap _tBitmap = new MarshalBitmap(size.Width, size.Height, data))
                {
                    _rBitmap = new Lazy<Bitmap>(() => new Bitmap(_tBitmap.Bitmap, (int)width, (int)width));
                    return _rBitmap.Value;
                }
            }
        }
    }
}
