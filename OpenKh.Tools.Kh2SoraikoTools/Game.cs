using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Input;

namespace OpenKh.Tools.Kh2SoraikoTools
{
    public class Game : GameWindow
    {
        public unsafe Game() : base(OpenTK.DisplayDevice.Default.Bounds.Width,
            OpenTK.DisplayDevice.Default.Bounds.Height, 
            new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4))
        {
            StaticConstants.ScreenWidth = OpenTK.DisplayDevice.Default.Bounds.Width;
            StaticConstants.ScreenHeight = OpenTK.DisplayDevice.Default.Bounds.Height;

            Size newSize = new Size((int)(Size.Width * 0.9f), (int)(Size.Height * 0.9f));
            Point newLocation = new Point(Location.X + (Size.Width - newSize.Width) / 2, Location.Y + (Size.Height - newSize.Height) / 2);

            Size = newSize;
            Location = newLocation;

            
        }
        Model m;

        protected override void OnLoad(EventArgs e)
        {
            StaticConstants.Init();
            GraphicFunctions.Init();

            //Camera.SetTarget(this.Namespace.Models[0]);

            m = new MDLX(@"Content\obj\WM_SYMBOL_TT.mdlx");

            Camera.Target = m;

            stp.Start();
            base.OnLoad(e);
        }

        KeyboardState oldKeyboardState;

        public static bool Skip = false;
        public static System.Diagnostics.Stopwatch stp = new Stopwatch();


        protected unsafe override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState keyboardState = Keyboard.GetState();


            //Skip = Mouse.GetState().X < 10;
            //double before = stp.Elapsed.TotalMilliseconds;

            Camera.Controls(keyboardState, oldKeyboardState);
            Camera.Update(this);

            m.Update(false);
            //Console.WriteLine((Skip?"HAUT ":"BAS ") + (stp.Elapsed.TotalMilliseconds-before));

            oldKeyboardState = keyboardState;
            base.OnUpdateFrame(e);
        }



        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.ClearColor(Color.Green);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);




            if (Camera.Target!=null)
            Camera.DestLookAt = Camera.Target.Skeleton.Matrices[0].ExtractTranslation();

            m.Draw(false);

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);


            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            /*GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferObject);
            base.OnUnload(e);*/
        }
    }

}
