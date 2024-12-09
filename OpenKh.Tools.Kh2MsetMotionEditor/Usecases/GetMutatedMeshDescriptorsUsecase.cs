using OpenKh.Engine.Parsers;
using System.Collections.Generic;
using System.Numerics;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public delegate IEnumerable<MeshDescriptor> GetMutatedMeshDescriptorsUsecase(
        Matrix4x4[]? matrices
    );
}
