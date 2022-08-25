using OpenKh.Command.MapGen.Interfaces;
using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xe.IO;
using BoundingBox = OpenKh.Kh2.Utils.BoundingBox;

namespace OpenKh.Command.MapGen.Utils
{
    public class DoctBuilder
    {
        public readonly Doct doct = new Doct();

        public List<ushort[]> vifPacketRenderingGroup { get; } = new List<ushort[]>();

        public DoctBuilder(ISpatialMeshCutter cutter)
        {
            var root = new ToTree(cutter).Root;
            WalkTree(root);
        }

        private class WalkResult
        {
            internal ushort entry1Idx;
        }

        private WalkResult WalkTree(Node node)
        {
            if (node.points != null)
            {
                var entry2Idx = doct.Entry2List.Count;
                var entry2 = new Doct.Entry2
                {
                    BoundingBox = node.bbox,
                };
                doct.Entry2List.Add(entry2);

                vifPacketRenderingGroup.Add(
                    node.points
                        .SelectMany(it => it.bigMesh.vifPacketIndices)
                        .ToArray()
                );

                var entry1Idx = doct.Entry1List.Count;
                var entry1 = new Doct.Entry1
                {
                    Entry2Index = Convert.ToUInt16(entry2Idx),
                    Entry2LastIndex = Convert.ToUInt16(entry2Idx + 1),
                };
                doct.Entry1List.Add(entry1);

                return new WalkResult
                {
                    entry1Idx = Convert.ToUInt16(entry1Idx),
                };
            }

            if (node.a != null)
            {
                var entry1Idx = doct.Entry1List.Count;
                var entry1 = new Doct.Entry1();
                doct.Entry1List.Add(entry1);

                var a = WalkTree(node.a);
                var b = WalkTree(node.b);

                entry1.Child1 = (short)a.entry1Idx;
                entry1.Child2 = (short)b.entry1Idx;
                entry1.BoundingBox = node.bbox;

                return new WalkResult
                {
                    entry1Idx = Convert.ToUInt16(entry1Idx),
                };
            }

            throw new Exception("Unexpected");
        }

        private class Node
        {
            internal Node a;
            internal Node b;

            internal CenterPointedMesh[] points;
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
                if (pair.Length == 2)
                {
                    var a = Walk(pair[0]);
                    var b = Walk(pair[1]);

                    return new Node
                    {
                        a = a,
                        b = b,

                        bbox = BoundingBox.Merge(a.bbox, a.bbox),
                    };
                }
                else
                {
                    var meshes = pair[0].Meshes.ToArray();

                    return new Node
                    {
                        points = meshes,

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
