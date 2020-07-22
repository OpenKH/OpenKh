using OpenKh.Command.MapTool.Models;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static OpenKh.Kh2.Coct;

namespace OpenKh.Command.MapGen.Utils
{
    public class CollisionBuilder
    {
        public readonly Coct coct = new Coct();
        public readonly Doct doct = new Doct();

        public CollisionBuilder(IEnumerable<BigMesh> bigMeshes)
        {
            var helper = new BuildHelper(coct);

            var firstIdx2 = coct.CollisionMeshList.Count;

            foreach (var mesh in bigMeshes
                .Where(it => !it.matDef.noclip)
            )
            {
                var firstIdx3 = coct.CollisionList.Count;

                foreach (var set in TriangleStripsToTriangleFans(mesh.triangleStripList))
                {
                    var quad = set.Count == 4;

                    var v1 = mesh.vertexList[set[0]];
                    var v2 = mesh.vertexList[set[1]];
                    var v3 = mesh.vertexList[set[2]];
                    var v4 = quad ? mesh.vertexList[set[3]] : Vector3.Zero;

                    coct.CompleteAndAdd(
                        new Collision
                        {
                            Vertex1 = helper.AllocateVertex(v1.X, -v1.Y, -v1.Z), // why -Y and -Z ?
                            Vertex2 = helper.AllocateVertex(v2.X, -v2.Y, -v2.Z),
                            Vertex3 = helper.AllocateVertex(v3.X, -v3.Y, -v3.Z),
                            Vertex4 = Convert.ToInt16(quad ? helper.AllocateVertex(v4.X, -v4.Y, -v4.Z) : -1),
                            SurfaceFlagsIndex = helper.AllocateSurfaceFlags(mesh.matDef.surfaceFlags),
                        },
                        inflate: 1
                    );
                }

                var lastIdx3 = coct.CollisionList.Count;

                var collisionMesh = coct.CompleteAndAdd(
                    new CollisionMesh
                    {
                        CollisionStart = Convert.ToUInt16(firstIdx3),
                        CollisionEnd = Convert.ToUInt16(lastIdx3),
                    }
                );

            }

            var lastIdx2 = coct.CollisionMeshList.Count;

            var firstGroup = coct.CompleteAndAdd(
                new CollisionMeshGroup
                {
                    CollisionMeshStart = Convert.ToUInt16(firstIdx2),
                    CollisionMeshEnd = Convert.ToUInt16(lastIdx2),
                }
            );

            // Entry2 index is tightly coupled to vifPacketRenderingGroup's index.
            // Thus do not add Entry2 unplanned.

            doct.Add(
                new Doct.Entry2
                {
                    BoundingBox = firstGroup.BoundingBox
                        .ToBoundingBox(),
                }
            );

            doct.CompleteAndAdd(
                new Doct.Entry1
                {
                    Entry2Index = Convert.ToUInt16(0),
                    Entry2LastIndex = Convert.ToUInt16(1),
                }
            );
        }

        private IEnumerable<IList<int>> TriangleStripsToTriangleFans(List<BigMesh.TriangleStrip> list)
        {
            foreach (var set in list)
            {
                var cx = set.vertexIndices.Count;
                if (cx == 3)
                {
                    yield return set.vertexIndices;
                }
                if (cx >= 4)
                {
                    for (int x = 4; x <= cx; x += 2)
                    {
                        yield return new int[]
                        {
                            set.vertexIndices[x - 4 + 0],
                            set.vertexIndices[x - 4 + 1],
                            set.vertexIndices[x - 4 + 3],
                            set.vertexIndices[x - 4 + 2],
                        };
                        if (x + 1 == cx)
                        {
                            yield return new int[]
                            {
                                set.vertexIndices[x - 2 + 0],
                                set.vertexIndices[x - 2 + 1],
                                set.vertexIndices[x - 2 + 2],
                            };
                        }
                    }
                }
            }
        }
    }
}
