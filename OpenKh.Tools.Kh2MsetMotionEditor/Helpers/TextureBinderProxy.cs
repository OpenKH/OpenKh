using Microsoft.Xna.Framework.Graphics;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class TextureBinderProxy : ITextureBinder
    {
        private readonly MonoGameImGuiBootstrap _bootstrap;

        public TextureBinderProxy(
            MonoGameImGuiBootstrap bootstrap
        )
        {
            _bootstrap = bootstrap;
        }

        public IntPtr BindTexture(Texture2D texture) =>
            _bootstrap.BindTexture(texture);

        public void RebindTexture(IntPtr id, Texture2D texture) =>
            _bootstrap.RebindTexture(id, texture);

        public void UnbindTexture(IntPtr id) =>
            _bootstrap.UnbindTexture(id);
    }
}
