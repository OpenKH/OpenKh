using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renders;
using System;

namespace OpenKh.Tools.LayoutEditor.Interfaces
{
    public interface ITextureBinder
    {
        IntPtr BindTexture(Texture2D texture);
        void UnbindTexture(IntPtr id);
    }

    public static class TextureBuilderExtensions
    {
        public static IntPtr BindTexture(
            this ITextureBinder textureBinder, ISpriteTexture spriteTexture)
        {
            var realSpriteTexture = (MonoSpriteDrawing.CSpriteTexture)spriteTexture;
            return textureBinder.BindTexture(realSpriteTexture.Texture);
        }
    }
}
