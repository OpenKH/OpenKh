using System.Collections.Generic;
using System.Numerics;

namespace OpenKh.Engine.Parsers.Kddf2
{
    class ExportedMesh
    {
        public List<Vector3> positionList = new List<Vector3>();
        public List<Vector2> uvList = new List<Vector2>();
        public List<Part> partList = new List<Part>();

        internal class Part
        {
            public List<TriangleRef> triangleRefList = new List<TriangleRef>();

            public ImmutableMesh meshRef;
        }
    }
}
