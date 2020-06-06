using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Extensions;
using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Game
{
    public static class Extensions
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
