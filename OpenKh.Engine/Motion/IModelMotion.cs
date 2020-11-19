using System.Numerics;

namespace OpenKh.Engine.Motion
{
    public interface IModelMotion
    {
        void ApplyMotion(Matrix4x4[] matrices);
    }
}
