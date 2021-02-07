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
        private class CameraMode
        {
            public float Fov { get; set; }
            public float Radius { get; set; }
            public float LockRadius { get; set; }
            public float RadiusMin { get; set; }
            public float RadiusMax { get; set; }
            public float ObjectiveUpCurve { get; set; }
        }

        private static readonly CameraMode CameraOutDoor = new CameraMode
        {
            Fov = 1.5f,
            Radius = 420f,
            LockRadius = 380f,
            RadiusMin = 250f,
            RadiusMax = 500f,
            ObjectiveUpCurve = 0.008f
        };
        private static readonly CameraMode CameraInDoor = new CameraMode
        {
            Fov = 1.0471975f,
            Radius = 600f,
            LockRadius = 520f,
            RadiusMin = 400f,
            RadiusMax = 700f,
            ObjectiveUpCurve = 0.005f
        };
        private static readonly CameraMode CameraCrowd = new CameraMode
        {
            Fov = 1.0471975f,
            Radius = 600f,
            LockRadius = 520f,
            RadiusMin = 400f,
            RadiusMax = 700f,
            ObjectiveUpCurve = 0.005f
        };
        private static readonly CameraMode CameraLightCycle = new CameraMode
        {
            Fov = 1.0471975f,
            Radius = 600f,
            LockRadius = 520f,
            RadiusMin = 400f,
            RadiusMax = 633f,
            ObjectiveUpCurve = 0.005f
        };
        private static readonly CameraMode[] CameraTypes = new CameraMode[]
        {
            CameraOutDoor,
            CameraInDoor,
            CameraCrowd,
            CameraLightCycle,
        };

        private Vector4 m_eyeTarget;
        private int _type;

        public TargetCamera(Camera camera)
        {
            Camera = camera;
            Type = 1;
        }

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

        public int Type
        {
            get => _type;
            set
            {
                Log.Info("{0}.{1}={2}", nameof(TargetCamera), nameof(Type), value);
                if (value < 0 || value >= CameraTypes.Length)
                    Log.Err("{0}.{1}={2} not valid", nameof(TargetCamera), nameof(Type), value);

                _type = value;
                var cameraMode = CameraTypes[value];
                Fov = DefaultFov = cameraMode.Fov;
                Radius = ObjectiveInitRadius = cameraMode.Radius;
                ObjectiveLockRadius = cameraMode.LockRadius;
                ObjectiveRadiusMin = cameraMode.RadiusMin;
                ObjectiveRadiusMax = cameraMode.RadiusMax;
                ObjectiveUpCurve = cameraMode.ObjectiveUpCurve;
            }
        }

        public void Update(Vector3 target, double deltaTime)
        {
            const bool Interpolate = false;

            AtTarget = new Vector4(
                target.X,
                -target.Y - 170f,
                -target.Z,
                1f);

            CalculateEyeTarget(AtTarget, Interpolate, deltaTime);
            Camera.FieldOfView = Fov;
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
