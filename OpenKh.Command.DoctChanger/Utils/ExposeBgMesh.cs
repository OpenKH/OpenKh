using Assimp;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.DoctChanger.Utils
{
    internal class ExposeBgMesh
    {
        private readonly Func<int, (Vector3D[] vertices, int[] indices)[]> _groupIdxToMeshDescs;

        public ExposeBgMesh(
            Func<int, (Vector3D[] vertices, int[] indices)[]> groupIdxToMeshDescs
        )
        {
            _groupIdxToMeshDescs = groupIdxToMeshDescs;
        }

        internal Mesh GetMesh(int groupIdx)
        {
            var mesh = new Mesh(PrimitiveType.Triangle);

            foreach (var set in _groupIdxToMeshDescs(groupIdx))
            {
                var topVertIdx = mesh.Vertices.Count;

                mesh.Vertices.AddRange(set.vertices);

                mesh.Faces.AddRange(
                    set.indices
                        .Buffer(3)
                        .Select(
                            triple => new Face(
                                triple
                                    .Select(idx => topVertIdx + idx)
                                    .ToArray()
                            )
                        )
                );
            }

            return mesh;
        }
    }
}
