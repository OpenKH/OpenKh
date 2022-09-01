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
        private readonly Func<int, MeshDescriptor[]> _groupIdxToMeshDescs;

        public ExposeBgMesh(
            Func<int, MeshDescriptor[]> groupIdxToMeshDescs
        )
        {
            _groupIdxToMeshDescs = groupIdxToMeshDescs;
        }

        internal Mesh GetMesh(int groupIdx)
        {
            var mesh = new Mesh(PrimitiveType.Triangle);

            foreach (var meshDesc in _groupIdxToMeshDescs(groupIdx))
            {
                var topVertIdx = mesh.Vertices.Count;

                mesh.Vertices.AddRange(
                    meshDesc.Vertices
                        .Select(vert => new Vector3D(vert.X, vert.Y, vert.Z))
                );
                mesh.Faces.AddRange(
                    meshDesc.Indices
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
