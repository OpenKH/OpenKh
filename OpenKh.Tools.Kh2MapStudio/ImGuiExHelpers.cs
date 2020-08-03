using System;
using System.Numerics;
using xna = Microsoft.Xna.Framework;
using OpenKh.Tools.Common.CustomImGui;

namespace OpenKh.Tools.Kh2MapStudio
{
    static class ImGuiExHelpers
    {
        public static void ForEdit2(string name, Func<xna.Vector2> getter, Action<xna.Vector2> setter)
        {
            var actualValue = getter();
            ImGuiEx.ForEdit2(name,
                () => new Vector2(actualValue.X, actualValue.Y),
                x => setter(new xna.Vector2(x.X, x.Y)));
        }

        public static void ForEdit3(string name, Func<xna.Vector3> getter, Action<xna.Vector3> setter)
        {
            var actualValue = getter();
            ImGuiEx.ForEdit3(name,
                () => new Vector3(actualValue.X, actualValue.Y, actualValue.Z),
                x => setter(new xna.Vector3(x.X, x.Y, x.Z)));
        }
    }
}
