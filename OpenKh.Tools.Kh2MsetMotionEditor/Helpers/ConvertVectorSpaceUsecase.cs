using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class ConvertVectorSpaceUsecase
    {
        public Func<Vector3, (Vector2 Position, float Scale, bool Visible)> GetFromLocalSpaceToWindowsPixelSpace(xna.Rectangle bounds, Matrix4x4 worldProjection)
        {
            return position =>
            {
                var vec4 = Vector4.Transform(position, worldProjection);
                var visible = 0 <= vec4.W;
                var vec2 = new Vector2(vec4.X / vec4.W, -vec4.Y / vec4.W);

                return (
                    new Vector2(
                        (vec2.X + 1) / 2 * bounds.Width + bounds.X,
                        (vec2.Y + 1) / 2 * bounds.Height + bounds.Y
                    ),
                    1.0f / vec4.W,
                    visible
                );
            };
        }
    }
}
