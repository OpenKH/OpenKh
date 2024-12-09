using System.Numerics;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public record FkIkMatrices(Matrix4x4[] Fk, Matrix4x4[] Ik)
    {
    }
}
