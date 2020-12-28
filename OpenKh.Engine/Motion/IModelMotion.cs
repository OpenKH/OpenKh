using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Numerics;

namespace OpenKh.Engine.Motion
{
    public interface IModelMotion
    {
        List<MeshDescriptor> MeshDescriptors { get; }

        List<Mdlx.Bone> Bones { get; } // Not a very good practice, but it's temporary

        Matrix4x4[] InitialPose { get; }

        void ApplyMotion(Matrix4x4[] matrices);
    }
}
