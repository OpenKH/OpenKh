using OpenKh.Engine.Parsers;
using System.Collections.Generic;
using System.Numerics;

namespace OpenKh.Engine.Motion
{
    public interface IModelMotion
    {
        List<MeshDescriptor> MeshDescriptors { get; }

        Matrix4x4[] InitialPose { get; }

        void ApplyMotion(Matrix4x4[] matrices);
    }
}
