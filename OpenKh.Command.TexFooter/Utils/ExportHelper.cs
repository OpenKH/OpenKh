using OpenKh.Command.TexFooter.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Command.TexFooter.Utils
{
    static class ExportHelper
    {
        internal static PerTexture AlsoExportImages(string outDir, string baseName, PerTexture perTexture)
        {
            foreach (var pair in perTexture.Textures)
            {
                var key = pair.Key;
                var textureFooterData = pair.Value;

                textureFooterData.TextureAnimationList?
                    .Select((it, index) => (it, index))
                    .ToList()
                    .ForEach(
                        pair =>
                        {
                            var src = pair.it._source;
                            var bitmap = SpriteImageUtil.ToBitmap(
                                src.BitsPerPixel,
                                src.SpriteWidth,
                                src.SpriteHeight,
                                src.NumSpritesInImageData,
                                src.SpriteStride,
                                src.SpriteImage
                            );
                            var pngFile = Path.Combine(outDir, $"{baseName}.footer-{key}-{pair.index}.png");
                            bitmap.Save(pngFile, ImageFormat.Png);

                            pair.it.SpriteImageFile = "./" + Path.GetFileName(pngFile);
                        }
                    );
            }

            return perTexture;
        }
    }
}
