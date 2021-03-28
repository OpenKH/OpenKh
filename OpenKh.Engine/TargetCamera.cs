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

        private const float FovDefault = 1.5f;
        private const float FovClose = (float)(Math.PI / (FovDefault * 2f));
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
            Fov = FovClose,
            Radius = 600f,
            LockRadius = 520f,
            RadiusMin = 400f,
            RadiusMax = 700f,
            ObjectiveUpCurve = 0.005f
        };
        private static readonly CameraMode CameraCrowd = new CameraMode
        {
            Fov = FovClose,
            Radius = 600f,
            LockRadius = 520f,
            RadiusMin = 400f,
            RadiusMax = 700f,
            ObjectiveUpCurve = 0.005f
        };
        private static readonly CameraMode CameraLightCycle = new CameraMode
        {
            Fov = FovClose,
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
        private Vector3 _targetPositionPrev;
        private int _type;

        public TargetCamera(Camera camera)
        {
            Camera = camera;
            Type = 0;
            Interpolate = true;
        }

        public Camera Camera { get; }
        public bool Interpolate { get; set; }
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

        public void Update(IEntity objTarget, double deltaTime, bool isYRotationLocked = false)
        {
            var targetPosition = objTarget.Position;

            AtTarget = new Vector4(
                targetPosition.X,
                -targetPosition.Y - 170f,
                -targetPosition.Z,
                1f);

            // This is not really the right way to know if the focused entity is actually moving
            var isEntityMoving = targetPosition.X != _targetPositionPrev.X ||
                targetPosition.Z != _targetPositionPrev.Z;
            _targetPositionPrev = targetPosition;
            if (isEntityMoving && !isYRotationLocked)
            {
                AdjustHorizontalRotation(objTarget, deltaTime);
                AdjustVerticalDefaultRotation(deltaTime);
            }

            CalculateEyeTarget(AtTarget, false, deltaTime);
            FovVTarget = new Vector4(Fov, WarpRadians(Roll), 0f, 1f);

            if (Interpolate)
            {
                At = InterpolateVector(At, AtTarget, AtTargetPrev, 30.0 * deltaTime * 0.1f, 2.5f, 0.5f, 1.0f);
                Eye = InterpolateVector(Eye, EyeTarget, EyeTargetPrev, 30.0 * deltaTime * 0.07f, 2.5f, 0.5f, 1.0f);
                FovV = InterpolateVector(FovV, FovVTarget, FovVTargetPrev, 30.0 * deltaTime * 0.07f, 2.5f, 0.5f, 0.001f);
            }
            else
            {
                Eye = EyeTarget;
                At = AtTarget;
                FovV = FovVTarget;
            }

            EyeTargetPrev = EyeTarget;
            AtTargetPrev = AtTarget;
            FovVTargetPrev = FovVTarget;

            Camera.FieldOfView = Fov;
            Camera.CameraPosition = Eye.ToVector3().Invert();
            Camera.CameraLookAt = At.ToVector3().Invert();
        }

        public void InstantlyRotateCameraToEntity(IEntity objTarget) =>
            YRotation = BackYRotation = GetYRotation(objTarget);

        private float GetYRotation(IEntity objTarget) =>
            WarpRadians((float)(Math.PI * 2 - objTarget.Rotation.Y));

        private void AdjustVerticalDefaultRotation(double deltaTime) =>
            AdjustVerticalRotation(ObjectiveInitRadius, deltaTime / 2.0);

        private void AdjustVerticalLockonRotation(double deltaTime) =>
            AdjustVerticalRotation(ObjectiveLockRadius, deltaTime);

        private void AdjustVerticalRotation(float objectiveRadius, double deltaTime)
        {
            if (Math.Abs(Radius - objectiveRadius) >= 1.0)
            {
                if (Radius > objectiveRadius)
                {
                    Radius = (float)(Radius - deltaTime * 60);
                    if (Radius <= ObjectiveRadiusMin)
                        Radius = ObjectiveRadiusMin;
                }
                else
                {
                    Radius = (float)(Radius + deltaTime * 60);
                    if (Radius >= ObjectiveRadiusMax)
                        Radius = ObjectiveRadiusMax;
                }
            }
        }

        private void AdjustHorizontalRotation(IEntity objTarget, double deltaTime)
        {
            const double Speed = Math.PI / 720.0;
            const double PlayerSpeedMul = Speed / 25.0;
            const float analogX = 0f;
            const float analogY = 0f;
            const float analogW = 1f;
            const float playerSpeed = 8f; // ???
            float objYRotation = GetYRotation(objTarget);
            var deltaFrame = deltaTime * 60.0;

            var speed = (Math.Abs(analogX) + 1.0) * (Math.Abs(analogY) + 1.0) * analogW * 4.0 *
                ((PlayerSpeedMul * (playerSpeed - 8.0)) + Speed) * deltaFrame;
            var rotation = InterpolateYRotation(YRotation, objYRotation, speed);
            YRotation = BackYRotation = rotation;
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

        private Vector4 InterpolateVector(
            Vector4 dst, Vector4 src, Vector4 srcPrev,
            double deltaTime,
            float springConst,
            float dampConst,
            float springLen)
        {
            var vDiff = dst - src;
            var vectorLength = vDiff.Length();
            if (vectorLength == 0)
                return dst;

            var v0 = vDiff * Vector4.Multiply(srcPrev - src, (float)deltaTime);
            var f0 = v0.X + v0.Y + v0.Z;
            var f2 = (springConst * (springLen - vectorLength)) + dampConst * f0 / vectorLength;
            return dst + Vector4.Multiply(vDiff, (float)(1f / vectorLength * f2 * deltaTime));
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
