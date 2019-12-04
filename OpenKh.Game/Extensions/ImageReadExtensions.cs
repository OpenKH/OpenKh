using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Extensions;
using OpenKh.Imaging;

namespace OpenKh.Game.Extensions
{
    public static class ImageReadExtensions
    {
        public static Texture2D CreateTexture(this IImageRead image, GraphicsDevice graphicsDevice)
        {
            var size = image.Size;
            var texture = new Texture2D(graphicsDevice, size.Width, size.Height);

            texture.SetData(image.AsRgba8888());
            
            return texture;
        }
    }
}
