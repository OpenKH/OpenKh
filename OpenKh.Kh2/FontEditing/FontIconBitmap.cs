using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2.FontEditing
{
    public class FontIconBitmap
    {
        public static byte[] WriteIconBitmap(IImageRead image)
        {
            if (false
                || image == null
                || image.PixelFormat != PixelFormat.Indexed8
                || image.Size.Width != 256
                || image.Size.Height != 160
            )
            {
                throw new Exception("Icon image expects 256 x 160 (Indexed8)");
            }

            var body = new byte[256 * 160 + 4 * 256];
            Buffer.BlockCopy(image.GetData(), 0, body, 0, 256 * 160);
            Buffer.BlockCopy(image.GetClut(), 0, body, 256 * 160, 4 * 256);
            return body;
        }
    }
}
