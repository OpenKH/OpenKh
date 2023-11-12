using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class ReplacePaletteAlphaUsecase
    {
        public IImageRead ReplacePaletteAlphaWith(IImageRead imageRead, byte fixedAlpha)
        {
            var clut = imageRead.GetClut();
            if (clut != null)
            {
                var newClut = new byte[clut.Length];
                Buffer.BlockCopy(clut, 0, newClut, 0, clut.Length);
                for (int x = 3; x < clut.Length; x += 4)
                {
                    newClut[x] = fixedAlpha;
                }
                clut = newClut;
            }

            return new PrivateProxy(
                imageRead.Size,
                imageRead.PixelFormat,
                clut,
                imageRead.GetData()
            );
        }

        private record PrivateProxy(
            Size Size,
            PixelFormat PixelFormat,
            byte[]? Clut,
            byte[] Data) : IImageRead
        {
            public byte[]? GetClut() => Clut;
            public byte[] GetData() => Data;
        }
    }
}
