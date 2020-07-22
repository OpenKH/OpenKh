using Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MapCollisionEditor.Services
{
    public static class CameraExtensions
    {
        public static void TraslateHorizontally(this Camera camera, float value) =>
            camera.CameraPosition += new Vector3(value, 0, 0);

        public static void TraslateVertically(this Camera camera, float value) =>
            camera.CameraPosition += new Vector3(0, value, 0);
    }
}
