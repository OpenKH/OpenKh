using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Interfaces
{
    public interface ITextureBinder
    {
        IntPtr BindTexture(Texture2D texture);
        void UnbindTexture(IntPtr id);
        void RebindTexture(IntPtr id, Texture2D texture);
    }
}
