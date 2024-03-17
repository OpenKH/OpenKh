using xna = Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace OpenKh.Engine.Monogame.Helpers
{
    public static class NumericsToXnaExtensions
    {
        public static xna.Matrix ToXnaMatrix(this Matrix4x4 it) => new xna.Matrix(
            it.M11, it.M12, it.M13, it.M14,
            it.M21, it.M22, it.M23, it.M24,
            it.M31, it.M32, it.M33, it.M34,
            it.M41, it.M42, it.M43, it.M44
        );
    }
}
