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
