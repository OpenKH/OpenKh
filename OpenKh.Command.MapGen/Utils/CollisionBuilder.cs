using OpenKh.Command.MapGen.Interfaces;
using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Utils;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using static OpenKh.Kh2.Coct;

namespace OpenKh.Command.MapGen.Utils
{
    public class CollisionBuilder
    {
        public readonly Coct coct = new Coct();

        public CollisionBuilder(ISpatialMeshCutter cutter)
        {
            var root = new ToTree(cutter).Root;
            var helper = new BuildHelper(coct);
            WalkTree(root, helper);
        }

        private class WalkResult
        {
            internal ushort collisionNodeIdx;
        }

        private WalkResult WalkTree(Node walkNode, BuildHelper helper)
        {
            var collisionNodeIdx = Convert.ToUInt16(coct.Nodes.Count);

            var collisionNode = new CollisionNode
            {
                Meshes = new List<CollisionMesh>(),
            };
            coct.Nodes.Add(collisionNode);

            if (walkNode.meshes.Any())
            {
                var collisionMesh = new CollisionMesh
                {
                    Collisions = new List<Collision>()
                };

                var vifPacketIndices = new List<ushort>();

                foreach (var point in walkNode.meshes.ToArray())
                {
                    var mesh = point.bigMesh;

                    vifPacketIndices.AddRange(mesh.vifPacketIndices);

                    foreach (var set in TriangleStripsToTriangleFans(mesh.triangleStripList))
                    {
                        var quad = set.Count == 4;

                        var v1 = mesh.vertexList[set[0]];
                        var v2 = mesh.vertexList[set[1]];
                        var v3 = mesh.vertexList[set[2]];
                        var v4 = quad ? mesh.vertexList[set[3]] : Vector3.Zero;

                        collisionMesh.Collisions.Add(coct.Complete(
                            new Collision
                            {
                                Vertex1 = helper.AllocateVertex(v1.X, -v1.Y, -v1.Z), // why -Y and -Z ?
                                Vertex2 = helper.AllocateVertex(v2.X, -v2.Y, -v2.Z),
                                Vertex3 = helper.AllocateVertex(v3.X, -v3.Y, -v3.Z),
                                Vertex4 = Convert.ToInt16(quad ? helper.AllocateVertex(v4.X, -v4.Y, -v4.Z) : -1),
                                Attributes = new Attributes() { Flags = mesh.matDef.surfaceFlags }
                            },
                            inflate: 1
                        ));
                    }
                }

                coct.Complete(collisionMesh);

                collisionNode.Meshes.Add(collisionMesh);
            }

            var childNodes = (walkNode.children ?? new Node[0])
                .Select(it => WalkTree(it, helper))
                .ToArray();

            if (8 < childNodes.Length)
            {
                throw new Exception("Unexpected");
            }

            collisionNode.Child1 = (short)(1 <= childNodes.Length ? childNodes[0].collisionNodeIdx : -1);
            collisionNode.Child2 = (short)(2 <= childNodes.Length ? childNodes[1].collisionNodeIdx : -1);
            collisionNode.Child3 = (short)(3 <= childNodes.Length ? childNodes[2].collisionNodeIdx : -1);
            collisionNode.Child4 = (short)(4 <= childNodes.Length ? childNodes[3].collisionNodeIdx : -1);
            collisionNode.Child5 = (short)(5 <= childNodes.Length ? childNodes[4].collisionNodeIdx : -1);
            collisionNode.Child6 = (short)(6 <= childNodes.Length ? childNodes[5].collisionNodeIdx : -1);
            collisionNode.Child7 = (short)(7 <= childNodes.Length ? childNodes[6].collisionNodeIdx : -1);
            collisionNode.Child8 = (short)(8 <= childNodes.Length ? childNodes[7].collisionNodeIdx : -1);
            collisionNode.BoundingBox = walkNode.bbox.ToBoundingBoxInt16();

            return new WalkResult
            {
                collisionNodeIdx = collisionNodeIdx,
            };
        }

        private static IEnumerable<IList<int>> TriangleStripsToTriangleFans(IList<BigMesh.TriangleStrip> list)
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

        private class Node
        {
            internal Node[] children;

            internal CenterPointedMesh[] meshes;
            internal BoundingBox bbox;
        }

        private class ToTree
        {
            internal Node Root { get; }

            public ToTree(ISpatialMeshCutter cutter)
            {
                Root = Walk(cutter);
            }

            private Node Walk(ISpatialMeshCutter cutter)
            {
                var pair = cutter.Cut().ToArray();

                if (8 < pair.Length)
                {
                    throw new Exception("Unexpected");
                }

                if (2 <= pair.Length)
                {
                    var children = pair
                        .Select(it => Walk(it))
                        .ToArray();

                    return new Node
                    {
                        children = children,
                        bbox = children
                            .Select(it => it.bbox)
                            .MergeAll(),
                    };
                }
                else
                {
                    var meshes = pair.Single().Meshes.ToArray();

                    return new Node
                    {
                        meshes = meshes,

                        bbox = BoundingBox.FromManyPoints(
                            meshes
                                .Select(point => point.bigMesh)
                                .SelectMany(bigMesh => bigMesh.vertexList)
                        ),
                    };
                }
            }
        }
    }
}
