using Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MapCollisionEditor.Services
{
    public class Camera
    {
        private float _fov;
        private float _aspectRatio;
        private float _nearClipPlane;
        private float _farClipPlane;
        private Vector3 _cameraPosition;
        private Vector3 _cameraLookAt;
        private Vector3 _cameraUp;
        private Matrix _projection;
        private Matrix _world;
        private bool _isProjectionInvalidated;
        private bool _isWorldInvalidated;
        private Vector3 _cameraYpr;

        public float FieldOfView
        {
            get => _fov;
            set
            {
                _fov = value;
                InvalidateProjection();
            }
        }

        public float NearClipPlane
        {
            get => _nearClipPlane;
            set
            {
                _nearClipPlane = value;
                InvalidateProjection();
            }
        }

        public float FarClipPlane
        {
            get => _farClipPlane;
            set
            {
                _farClipPlane = value;
                InvalidateProjection();
            }
        }

        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
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
                var matrix = Matrix.CreateFromYawPitchRoll(value.X / 180.0f * 3.14159f, value.Y / 180.0f * 3.14159f, value.Z / 180.0f * 3.14159f);
                CameraLookAt = Vector3.Transform(new Vector3(1, 0, 0), matrix);
            }
        }

        public Matrix Projection
        {
            get
            {
                if (_isProjectionInvalidated)
                    CalculateProjection();
                return _projection;
            }
        }

        public Matrix World
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
            _fov = 1.5f;
            AspectRatio = 512.0f / 448.0f;
            NearClipPlane = 1;
            FarClipPlane = int.MaxValue;
            CameraUp = new Vector3(0, 1, 0);
            CameraRotationYawPitchRoll = new Vector3(-90, 0, 10);
        }

        private void InvalidateProjection() => _isProjectionInvalidated = true;
        private void ValidateProjection() => _isProjectionInvalidated = false;
        private void InvalidateWorld() => _isWorldInvalidated = true;
        private void ValidateWorld() => _isWorldInvalidated = false;

        private void CalculateProjection()
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(
                _fov, _aspectRatio, _nearClipPlane, _farClipPlane);

            ValidateProjection();
        }

        private void CalculateWorld()
        {
            _world = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraLookAt, CameraUp);

            ValidateWorld();
        }
    }
}
