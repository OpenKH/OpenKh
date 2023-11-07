using OpenKh.Engine.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public delegate IEnumerable<MeshDescriptor> GetMutatedMeshDescriptorsUsecase(
        Matrix4x4[]? matrices
    );
}
