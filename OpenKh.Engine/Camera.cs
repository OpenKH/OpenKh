using System;
using System.Numerics;

namespace OpenKh.Engine
{
    public class Camera
    {
        private bool _isEventMode;
        private float _fov;
        private float _aspectRatio;
        private Vector3 _cameraPosition;
        private Vector3 _cameraLookAt;
        private Vector3 _cameraLookAtX;
        private Vector3 _cameraLookAtY;
        private Vector3 _cameraLookAtZ;
        private Vector3 _cameraUp;
        private Matrix4x4 _projection;
        private Matrix4x4 _world;
        private bool _isProjectionInvalidated;
        private bool _isWorldInvalidated;
        private Vector3 _cameraYpr;

        public bool IsEventMode
        {
            get => _isEventMode;
            set
            {
                if (_isEventMode == value)
                    return;

                _isEventMode = value;
                InvalidateProjection();
            }
        }

        public float FieldOfView
        {
            get => _fov;
            set
            {
                if (_fov == value)
                    return;
                _fov = value;
                InvalidateProjection();
            }
        }

        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (_aspectRatio == value)
                    return;
                _aspectRatio = value;
                InvalidateProjection();
            }
        }

        public Vector3 CameraPosition
        {
            get => _cameraPosition;
            set
            {
                _cameraPosition = value;
                CameraLookAt = CameraPosition + CameraLookAtX;
                InvalidateWorld();
            }
        }

        public Vector3 CameraLookAt
        {
            get => _cameraLookAt;
            set
            {
                _cameraLookAt = value;
                InvalidateWorld();
            }
        }

        public Vector3 CameraLookAtX
        {
            get => _cameraLookAtX;
            set
            {
                _cameraLookAtX = value;
                CameraLookAt = CameraPosition + CameraLookAtX;
                InvalidateWorld();
            }
        }

        public Vector3 CameraLookAtY
        {
            get => _cameraLookAtY;
            set
            {
                _cameraLookAtY = value;
                InvalidateWorld();
            }
        }

        public Vector3 CameraLookAtZ
        {
            get => _cameraLookAtZ;
            set
            {
                _cameraLookAtZ = value;
                InvalidateWorld();
            }
        }

        public Vector3 CameraUp
        {
            get => _cameraUp;
            set
            {
                _cameraUp = value;
                InvalidateWorld();
            }
        }

        public Vector3 CameraRotationYawPitchRoll
        {
            get => _cameraYpr;
            set
            {
                _cameraYpr = value;
                var matrix = Matrix4x4.CreateFromYawPitchRoll(
                    (float)(value.X * Math.PI / 180.0),
                    (float)(value.Y * Math.PI / 180.0),
                    (float)(value.Z * Math.PI / 180.0));
                CameraLookAtX = Vector3.Transform(new Vector3(1, 0, 0), matrix);
                CameraLookAtY = Vector3.Transform(new Vector3(0, 0, 1), matrix);
                CameraLookAtZ = Vector3.Transform(new Vector3(0, 1, 0), matrix);
            }
        }

        public Matrix4x4 Projection
        {
            get
            {
                if (_isProjectionInvalidated)
                    CalculateProjection();
                return _projection;
            }
        }

        public Matrix4x4 World
        {
            get
            {
                if (_isWorldInvalidated)
                    CalculateWorld();
                return _world;
            }
        }

        public Camera()
        {
            FieldOfView = 1.5f;
            AspectRatio = 640f / 480f;
            CameraUp = new Vector3(0, 1, 0);
            CameraRotationYawPitchRoll = new Vector3(-90, 0, 10);
        }

        private void InvalidateProjection() => _isProjectionInvalidated = true;
        private void ValidateProjection() => _isProjectionInvalidated = false;
        private void InvalidateWorld() => _isWorldInvalidated = true;
        private void ValidateWorld() => _isWorldInvalidated = false;

        private void CalculateProjection()
        {
            const float NearClipPlane = 1f;
            const float FarClipPlane = 4000000f;

            if (!_isEventMode)
            {
                const double ReferenceWidth = 640f;
                const double ReferenceHeight = 480f;

                var srcz = ReferenceWidth / 2.0 / Math.Tan(_fov / 2.0);
                var actualAspectRatio = 1.0 / _aspectRatio / (ReferenceHeight / ReferenceWidth);
                var width = ReferenceWidth / (srcz * actualAspectRatio);
                var height = ReferenceHeight / srcz;
                _projection = Matrix4x4.CreatePerspective((float)width, (float)height, NearClipPlane, FarClipPlane);
            }
            else
                _projection = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, NearClipPlane, FarClipPlane);

            ValidateProjection();
        }

        private void CalculateWorld()
        {
            _world = Matrix4x4.CreateLookAt(
                new Vector3(-CameraPosition.X, CameraPosition.Y, CameraPosition.Z),
                new Vector3(-CameraLookAt.X, CameraLookAt.Y, CameraLookAt.Z),
                CameraUp);

            ValidateWorld();
        }
    }
}
