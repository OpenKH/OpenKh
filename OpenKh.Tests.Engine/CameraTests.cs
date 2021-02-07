using OpenKh.Engine;
using System;
using System.Numerics;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class CameraTests
    {
        [Theory]
        [InlineData(512f, 384f, 1.5f, 1.073426127f, 1.431234717f)]
        [InlineData(512f, 384f, 1.047197461f, 1.7320510149002075f, 2.309401512f)]
        [InlineData(512f, 288f, 1.5f, 0.805069625377655f, 1.431234717f)]
        [InlineData(512f, 288f, 1.047197461f, 1.299038291f, 2.309401512f)]
        [InlineData(640f, 480f, 1.5f, 1.073426127f, 1.431234717f)]
        [InlineData(1920f, 1080f, 1.047197461f, 1.299038291f, 2.309401512f)]
        public void SetProjectionMatrixTest(
            float width, float height, float fov,
            float expectedM11, float expectedM22)
        {
            var camera = new Camera();
            camera.AspectRatio = width / height;
            camera.FieldOfView = fov;

            AssertMatrix(new Matrix4x4(
                expectedM11, 0, 0, 0,
                0, expectedM22, 0f, 0,
                0, 0f, -1f, -1f,
                0, 0, -1f, 0
                ), camera.Projection);
        }

        [Fact]
        public void SetWorldMatrixTest()
        {
            var camera = new Camera();
            camera.CameraPosition = new Vector3(0f, 251.1999969f, -920f);
            camera.CameraLookAt = new Vector3(0f, 170f, -500f);
            camera.CameraUp = new Vector3(0f, 1f, 0f);

            AssertMatrix(new Matrix4x4(
                -1, 0, 0, 0,
                0, 0.9818192f, 0.189818367f, 0,
                0, 0.189818367f, -0.9818192f, 0,
                0, -72.00009f, -950.956f, 1
                ), camera.World);
        }

        [Theory]
        [InlineData(0f, 0f, 0f, 520f, -500f)]
        [InlineData(420f, 0f, 0f, 251.19999f, -920f)]
        [InlineData(420f, 1f, -353.4177246f, 251.199997f, -726.927002f)]
        public void TargetCameraSetsPositionTest(
            float radius, float yRotation,
            float expectedX, float expectedY, float expectedZ)
        {
            var targetPosition = new Vector3(0f, 0f, -500f);

            var camera = new Camera();
            var targetCamera = new TargetCamera(camera)
            {
                Type = 0,
                Radius = radius,
                YRotation = yRotation,
                BackYRotation = yRotation
            };
            targetCamera.Update(targetPosition, 0);

            var expected = new Vector3(expectedX, expectedY, expectedZ);
            AssertVector3(expected, camera.CameraPosition);
        }

        [Fact]
        public void TargetCameraSetCorrectValuesToCamera()
        {
            var targetPosition = new Vector3(0f, 150f, 0f);

            var camera = new Camera();
            var targetCamera = new TargetCamera(camera)
            {
                Type = 0,
                Radius = 420f,
                YRotation = 0f,
                BackYRotation = 0f
            };
            targetCamera.Update(targetPosition, 0);

            var expectedPosition = new Vector3(0f, 401.2f, -420);
            var expectedLookAt = new Vector3(0f, 320f, 0f);
            AssertVector3(expectedPosition, camera.CameraPosition);
            AssertVector3(expectedLookAt, camera.CameraLookAt);
        }

        private void AssertVector3(Vector3 expected, Vector3 actual)
        {
            const int Precision = 3;
            Assert.Equal(expected.X, actual.X, Precision);
            Assert.Equal(expected.Y, actual.Y, Precision);
            Assert.Equal(expected.Z, actual.Z, Precision);
        }

        private void AssertMatrix(Matrix4x4 expected, Matrix4x4 actual)
        {
            const int Precision = 3;
            Assert.Equal(expected.M11, actual.M11, Precision);
            Assert.Equal(expected.M12, actual.M12, Precision);
            Assert.Equal(expected.M13, actual.M13, Precision);
            Assert.Equal(expected.M14, actual.M14, Precision);
            Assert.Equal(expected.M21, actual.M21, Precision);
            Assert.Equal(expected.M22, actual.M22, Precision);
            Assert.Equal(expected.M23, actual.M23, Precision);
            Assert.Equal(expected.M24, actual.M24, Precision);
            Assert.Equal(expected.M31, actual.M31, Precision);
            Assert.Equal(expected.M32, actual.M32, Precision);
            Assert.Equal(expected.M33, actual.M33, Precision);
            Assert.Equal(expected.M34, actual.M34, Precision);
            Assert.Equal(expected.M41, actual.M41, Precision);
            Assert.Equal(expected.M42, actual.M42, Precision);
            Assert.Equal(expected.M43, actual.M43, Precision);
            Assert.Equal(expected.M44, actual.M44, Precision);
        }
    }
}
