using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Interfaces
{
    public interface ITextureBinder
    {
        IntPtr BindTexture(Texture2D texture);
        void UnbindTexture(IntPtr id);
        void RebindTexture(IntPtr id, Texture2D texture);
    }
}
