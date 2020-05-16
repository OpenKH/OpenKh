using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Input;

namespace OpenKh.Tools.Kh2SoraikoTools
{
    public static class StaticConstants
    {
        /* SYSTEM */
        public static string ProcessDirectory;
        public static System.Globalization.CultureInfo en;

        /* GRAPHICS */
        public static int ScreenWidth = 1600;
        public static int ScreenHeight = 900;
        public static int FrameBuffer = -1;
        public static int RenderedTexture = -1;
        public static byte[] RenderedTexturePixels;
        public static IntPtr RenderedTexturePixels_IPTR;
        public static int whiteTexture_1x1;

        public static void Init()
        {
            en = new System.Globalization.CultureInfo("en-US");
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(1,1);
            bmp.SetPixel(0, 0, System.Drawing.Color.White);
            whiteTexture_1x1 = GraphicFunctions.LoadTexture(bmp);
            ProcessDirectory = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            RenderedTexturePixels = new byte[3840 * 2160 * 3];
            RenderedTexturePixels_IPTR = GCHandle.ToIntPtr(GCHandle.Alloc(RenderedTexturePixels));


            /*GL.GenFramebuffers(1, out StaticConstants.FrameBuffer);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, StaticConstants.FrameBuffer);

            GL.GenTextures(1, out StaticConstants.RenderedTexture);
            GL.BindTexture(TextureTarget.Texture2D, StaticConstants.RenderedTexture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 1024, 768, 0, PixelFormat.Rgb, PixelType.UnsignedByte, RenderedTexturePixels_IPTR);
       
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { 9728 });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { 9728 });

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.Aux0, RenderedTexture, 0);


            DrawBuffersEnum[] DrawBuffers = new DrawBuffersEnum[] { DrawBuffersEnum.Aux0 };
            GL.DrawBuffers(1, DrawBuffers);*/


        }
    }
}
