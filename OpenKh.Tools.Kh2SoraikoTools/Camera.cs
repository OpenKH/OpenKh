using System;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Input;

namespace OpenKh.Tools.Kh2SoraikoTools
{
	public static class Camera
	{
		static int ViewPortWidth = 0;
		static int ViewPortHeight = 0;

		
		static float Yaw = 0;
        public static float DestYaw = 0;

		public static float MinPitch = (float)(Math.PI * -0.49);
		public static float MaxPitch = (float)(Math.PI * 0.49);

		public static float Pitch = 0;
		static float DestPitch = 0;
		
		static float Roll = 0;
		static float DestRoll = 0f;

		static float Distance = 100f;
		static float DestDistance = 100f;

		static float FieldOfView = MathHelper.RadiansToDegrees(1.5f);
		static float DestFieldOfView = MathHelper.RadiansToDegrees(1.5f);

        public static Vector3 LookAt = Vector3.Zero;
		public static Vector3 DestLookAt = Vector3.Zero;
		public static Vector3 Position = Vector3.Zero;

		static float translateSpeed = 1f;
		static float rotateSpeed = 1f;

        public static Matrix3 YawMatrix = Matrix3.Identity;
        public static Matrix3 PitchMatrix = Matrix3.Identity;
        public static Matrix3 YawPitchMatrix = Matrix3.Identity;


        public static Matrix4  ViewMatrix = Matrix4 .Identity;
        public static Matrix4  ProjectionMatrix = Matrix4 .Identity;
        public static Matrix4  ProjectionMatrixMap = Matrix4 .Identity;
        public static Matrix4  ProjectionMatrixSkyBox = Matrix4 .Identity;

		public static float TranslateSpeed
		{
			get { return translateSpeed; }
			set
			{
				if (value < -60 || value > 60)
					return;
				translateSpeed = value;
			}
		}
		public static float RotateSpeed
		{
			get { return rotateSpeed; }
			set
			{
				if (value < -60 || value > 60)
					return;
				rotateSpeed = value;
			}
		}

		public static void Controls(KeyboardState keyboardState, KeyboardState oldKeyboardState)
		{
			if (keyboardState.IsKeyDown(Key.Keypad4))
				DestYaw += Vector3.Transform(Vector3.UnitX*0.1f, Matrix3.CreateRotationZ(Roll)).X;

			if (keyboardState.IsKeyDown(Key.Keypad6))
				DestYaw -= Vector3.Transform(Vector3.UnitX * 0.1f, Matrix3.CreateRotationZ(Roll)).X;

            if (keyboardState.IsKeyDown(Key.Keypad8))
			{
				DestPitch += 0.1f;
                if (DestPitch > MaxPitch)
                    DestPitch = MaxPitch;
			}

			if (keyboardState.IsKeyDown(Key.Keypad2))
			{
				DestPitch -= 0.1f;
                if (DestPitch < MinPitch)
                    DestPitch = MinPitch;
            }
		}

        public static Model Target = null;

        public static void SetTarget(Model model)
        {
            if (model != Target)
            {
                if (Target==null)
                {
                    Camera.Yaw = model.DestRotation + MathHelper.Pi;
                    Camera.DestYaw = model.DestRotation + MathHelper.Pi;
                    Camera.LookAt = model.Skeleton.Matrices[0].ExtractTranslation();
                    recreateViewMatrix = true;
                }
                Target = model;
            }
        }

        public static bool recreateViewMatrix = true;
        public static bool recreateProjectionMatrix = true;

        public static void Update(GameWindow Window)
        {
            if (Target==null)
            {
                recreateViewMatrix = true;
            }
            else
            {
                Camera.DestLookAt = Target.Skeleton.Matrices[0].ExtractTranslation();

                float diffYaw = (DestYaw - Yaw);
                float diffPitch = (DestPitch - Pitch);
                float diffRoll = (DestRoll - Roll);

                if (Math.Abs(diffYaw) > 0.000001)
                {
                    Yaw += diffYaw * (RotateSpeed / 30f);
                    recreateViewMatrix = true;
                }

                if (Math.Abs(diffPitch) > 0.000001)
                {
                    Pitch += diffPitch * (RotateSpeed / 30f);
                    recreateViewMatrix = true;
                }
                if (Math.Abs(diffRoll) > 0.000001)
                {
                    Roll += diffRoll * (RotateSpeed / 30f);
                    recreateViewMatrix = true;
                }

                Vector3 diffLookAt = DestLookAt - LookAt;
                if (Vector3.Distance(diffLookAt, Vector3.Zero) > 0.000001)
                {
                    LookAt += diffLookAt * (TranslateSpeed / 30f);
                    recreateViewMatrix = true;
                }

                float diffDistance = DestDistance - Distance;
                if (Math.Abs(diffDistance) > 0.000001)
                {
                    Distance += diffDistance * (TranslateSpeed / 30f);
                    recreateViewMatrix = true;
                }
            }



			float diffFieldOfView = DestFieldOfView - FieldOfView;
			if (Math.Abs(diffFieldOfView) > 0.000001)
			{
				FieldOfView += diffFieldOfView * (TranslateSpeed / 30f);
				recreateViewMatrix = true;
			}
			
			if (recreateViewMatrix)
            {
                YawMatrix = Matrix3.CreateRotationY(Yaw);
                PitchMatrix = Matrix3.CreateRotationX(Pitch);
                YawPitchMatrix = PitchMatrix * YawMatrix;
                
                Position = LookAt + Vector3.Transform(Vector3.UnitZ * Distance, YawPitchMatrix);

                ViewMatrix = Matrix4 .LookAt(Position, LookAt, Vector3.UnitY);
			}


			recreateProjectionMatrix = ViewPortWidth != Window.Bounds.Width || ViewPortHeight != Window.Bounds.Height;


			if (recreateProjectionMatrix)
            {


                ProjectionMatrix = Matrix4 .CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), Window.Bounds.Width / (float)Window.Bounds.Height,1f, 100000f);
                ProjectionMatrixMap = Matrix4 .CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), Window.Bounds.Width / (float)Window.Bounds.Height, 1f, 20000f);
				ProjectionMatrixSkyBox = Matrix4 .CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), Window.Bounds.Width / (float)Window.Bounds.Height, 20000,100000f);
			}
			
			ViewPortWidth = Window.Bounds.Width;
			ViewPortHeight = Window.Bounds.Height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref ProjectionMatrix);

            GL.MatrixMode(MatrixMode.Modelview0Ext);
            GL.LoadMatrix(ref ViewMatrix);
        }
	}
}
