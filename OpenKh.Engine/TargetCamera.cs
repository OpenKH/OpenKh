using OpenKh.Common;
using System;
using System.Numerics;

namespace OpenKh.Engine
{
    public static class VectorHelpers
    {
        public static Vector3 ToVector3(this Vector4 v) =>
            new Vector3(v.X, v.Y, v.Z);

        public static Vector3 Invert(this Vector3 v) =>
            new Vector3(-v.X, -v.Y, -v.Z);
    }

    public class TargetCamera
    {
        public TargetCamera(Camera camera)
        {
            Camera = camera;
            Fov = 1.5f;
            Radius = 420f;
            ObjectiveLockRadius = 380f;
            ObjectiveRadiusMin = 250f;
            ObjectiveRadiusMax = 500f;
            ObjectiveUpCurve = 0.008f;
        }

        private Vector4 m_eyeTarget;

        public Camera Camera { get; }
        public Vector4 At { get; set; }
        public Vector4 Eye { get; set; }
        public Vector4 FovV { get; set; }
        public float Fov { get; set; }
        public float Roll { get; set; }
        public float Radius { get; set; }
        public float YRotation { get; set; }
        public float BackYRotation { get; set; }

        public Vector4 EyeTarget
        {
            get => m_eyeTarget;
            set => m_eyeTarget = value;
        }

        public Vector4 EyeTargetPrev { get; set; }
        public Vector4 AtTarget { get; set; }
        public Vector4 AtTargetPrev { get; set; }
        public Vector4 FovVTarget { get; set; }
        public Vector4 FovVTargetPrev { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float ObjectiveInitRadius { get; set; }
        public float ObjectiveLockRadius { get; set; }
        public float ObjectiveRadiusMin { get; set; }
        public float ObjectiveRadiusMax { get; set; }
        public float ObjectiveUpCurve { get; set; }
        public float DefaultFov { get; set; }
        public float DefaultRoll { get; set; }

        public void Update(Vector3 target, double deltaTime)
        {
            const bool Interpolate = false;

            AtTarget = new Vector4(
                target.X,
                -target.Y - 170f,
                -target.Z,
                1f);

            CalculateEyeTarget(AtTarget, Interpolate, deltaTime);
            Camera.CameraPosition = EyeTarget.ToVector3().Invert();
            Camera.CameraLookAt = AtTarget.ToVector3().Invert();

            if (Interpolate)
            {
                Eye = EyeTargetPrev = EyeTarget;
                At = AtTargetPrev = AtTarget;
                FovV = FovVTargetPrev = FovVTarget =
                    new Vector4(Fov, WarpRadians(Roll), 0f, 1f);
            }
        }

        private void CalculateEyeTarget(Vector4 atTarget, bool interpolate, double deltaTime)
        {
            const double TurnSpeed = 10.4719752;

            var radiusDiff = Radius - ObjectiveRadiusMin;
            if (interpolate)
                YRotation = InterpolateYRotation(YRotation, BackYRotation, TurnSpeed * deltaTime);
            else
                YRotation = BackYRotation;

            EyeTarget = atTarget + Vector4.Transform(
                new Vector4(0, 0, Radius, 0), Matrix4x4.CreateRotationY(YRotation));
            m_eyeTarget.Y = -((radiusDiff * radiusDiff * ObjectiveUpCurve) - (atTarget.Y + 150.0f));
        }

        private float InterpolateYRotation(float src, float dst, double speed)
        {
            var diff = dst - src;
            double actualSpeed;
            if (WarpRadians(diff) >= -speed)
            {
                if (WarpRadians(diff) <= speed)
                    actualSpeed = WarpRadians(diff);
                else
                    actualSpeed = speed;
            }
            else
                actualSpeed = -speed;

            return WarpRadians((float)(src + actualSpeed));
        }

        private float WarpRadians(float radians)
        {
            const float PI_2 = (float)(Math.PI * 2);
            if (radians < -Math.PI)
            {
                do
                {
                    radians += PI_2;
                } while (radians < -Math.PI);
            }
            else if (Math.PI < radians)
            {
                do
                {
                    radians -= PI_2;
                } while (radians > Math.PI);
            }

            return radians;
        }
    }
}
