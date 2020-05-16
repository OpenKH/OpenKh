using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Input;

namespace OpenKh.Tools.Kh2SoraikoTools
{
    public static class GraphicFunctions
    {
        public static bool LastTextureAlpha = false;

        public static List<int> Textures = new List<int>(0);
        public static List<System.Drawing.Size> Textures_Bounds = new List<System.Drawing.Size>(0);
        public static List<IntPtr> Textures_IntPtrs = new List<IntPtr>(0);


        public static int LoadTexture(System.Drawing.Bitmap bitmap)
        {
            int depth = System.Drawing.Bitmap.GetPixelFormatSize(bitmap.PixelFormat);
            if (depth != 32)
            {
                bitmap = bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }

            int tex;
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out tex);
            Textures.Add(tex);

            GL.BindTexture(TextureTarget.Texture2D, tex);

            
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            Textures_Bounds.Add(new System.Drawing.Size(bitmap.Width, bitmap.Height));





            PixelInternalFormat format = PixelInternalFormat.Rgba;

            byte[] Pixels = new byte[bitmap.Width * bitmap.Height * 4];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, Pixels, 0, Pixels.Length);

            LastTextureAlpha = false;


            Textures_IntPtrs.Add(data.Scan0);

            GL.TexImage2D(TextureTarget.Texture2D, 0, format, bitmap.Width, bitmap.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            for (int i = 0; i < Pixels.Length; i+=4)
            {
                if (Pixels[i + 3] < 250)
                {
                    LastTextureAlpha = true;
                    break;
                }
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }
        public static void Init()
        {
            GL.Enable(EnableCap.Multisample);
            
        }

    }
}
