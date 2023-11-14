using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public record FkIkMatrices(Matrix4x4[] Fk, Matrix4x4[] Ik)
    {
    }
}
