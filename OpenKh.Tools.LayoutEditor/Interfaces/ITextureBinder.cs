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
        void RebindTexture(IntPtr id, Texture2D texture);
    }

    public static class TextureBuilderExtensions
    {
        public static IntPtr BindTexture(
            this ITextureBinder textureBinder, ISpriteTexture spriteTexture)
        {
            var realSpriteTexture = (MonoSpriteDrawing.CSpriteTexture)spriteTexture;
            return textureBinder.BindTexture(realSpriteTexture.Texture);
        }

        public static void RebindTexture(
            this ITextureBinder textureBinder, IntPtr id, ISpriteTexture spriteTexture)
        {
            var realSpriteTexture = (MonoSpriteDrawing.CSpriteTexture)spriteTexture;
            textureBinder.RebindTexture(id, realSpriteTexture.Texture);
        }
    }
}
