using Microsoft.Xna.Framework;
using OpenKh.Engine.MonoGame;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using static OpenKh.Tools.Kh2MapStudio.ImGuiExHelpers;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class CameraWindow
    {
        public static bool Run(Camera camera) => ForHeader("Camera", () =>
        {
            ForEdit3("Position", () => camera.CameraPosition, x => camera.CameraPosition = x);
            ForEdit2("Rotation",
                () => new Vector2(-camera.CameraRotationYawPitchRoll.X, -camera.CameraRotationYawPitchRoll.Z),
                x => camera.CameraRotationYawPitchRoll = new Vector3(
                    -x.X, camera.CameraRotationYawPitchRoll.Y, -x.Y));
        });
    }
}
