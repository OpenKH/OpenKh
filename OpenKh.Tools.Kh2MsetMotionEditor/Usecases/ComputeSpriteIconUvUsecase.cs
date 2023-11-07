using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class ComputeSpriteIconUvUsecase
    {
        public (Vector2 Uv0, Vector2 Uv1) Compute(int index)
        {
            var x = index & 15;
            var y = (index >> 4) & 15;

            return (
                new Vector2(1.0f / 16 * x, 1.0f / 16 * y),
                new Vector2(1.0f / 16 * (x + 1), 1.0f / 16 * (y + 1))
            );
        }

        public xna.Rectangle ComputeDrawSourceRect(int index, int width, int height)
        {
            var x = index & 15;
            var y = (index >> 4) & 15;

            return new xna.Rectangle(
                width / 16 * x,
                height / 16 * y,
                width / 16,
                height / 16
            );
        }
    }
}
