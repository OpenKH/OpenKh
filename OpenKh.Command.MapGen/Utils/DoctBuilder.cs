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

        public DoctBuilder(IEnumerable<BigMesh> bigMeshes)
        {
            var bsp = new BSP(
                bigMeshes
                    .Where(mesh => !mesh.matDef.noclip)
                    .Select(mesh => new CenterPointedMesh(mesh))
                    .ToArray()
            );

            var root = new BSPToTree(bsp).Root;
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

        private class CenterPointedMesh
        {
            public BigMesh bigMesh;

            public Vector3 centerPoint;

            public CenterPointedMesh(BigMesh bigMesh)
            {
                this.bigMesh = bigMesh;

                centerPoint = GetCenter(
                    bigMesh.triangleStripList
                        .SelectMany(triangleStrip => triangleStrip.vertexIndices)
                        .Select(index => bigMesh.vertexList[index])
                );
            }

            public override string ToString() => centerPoint.ToString();

            private static Vector3 GetCenter(IEnumerable<Vector3> positions)
            {
                double x = 0, y = 0, z = 0;
                int n = 0;
                foreach (var one in positions)
                {
                    ++n;
                    x += one.X;
                    y += one.Y;
                    z += one.Z;
                }
                return new Vector3(
                    (float)(x / n),
                    (float)(y / n),
                    (float)(z / n)
                );
            }
        }


        /// <summary>
        /// Binary separated partitions
        /// </summary>
        class BSP
        {
            public CenterPointedMesh[] Points { get; }

            public BSP(CenterPointedMesh[] points)
            {
                Points = points;
            }

            public override string ToString() => $"{Points.Length:#,##0} points";

            public BSP[] Split()
            {
                if (Points.Length >= 20)
                {
                    var range = new Range(Points);
                    if (range.yLen >= range.xLen)
                    {
                        if (range.zLen >= range.yLen)
                        {
                            // z-cut
                            return new BSP[]
                            {
                                new BSP(Points.Where(it => it.centerPoint.Z >= range.zCenter).ToArray()),
                                new BSP(Points.Where(it => it.centerPoint.Z < range.zCenter).ToArray()),
                            };
                        }
                        else
                        {
                            // y-cut
                            return new BSP[]
                            {
                                new BSP(Points.Where(it => it.centerPoint.Y >= range.yCenter).ToArray()),
                                new BSP(Points.Where(it => it.centerPoint.Y < range.yCenter).ToArray()),
                            };
                        }
                    }
                    else
                    {
                        // x-cut
                        return new BSP[]
                        {
                            new BSP(Points.Where(it => it.centerPoint.X >= range.xCenter).ToArray()),
                            new BSP(Points.Where(it => it.centerPoint.X < range.xCenter).ToArray()),
                        };
                    }
                }
                return new BSP[] { this };
            }

            class Range
            {
                public float xMin = float.MaxValue;
                public float xMax = float.MinValue;
                public float yMin = float.MaxValue;
                public float yMax = float.MinValue;
                public float zMin = float.MaxValue;
                public float zMax = float.MinValue;

                public float xLen;
                public float yLen;
                public float zLen;

                public float xCenter;
                public float yCenter;
                public float zCenter;

                public Range(CenterPointedMesh[] points)
                {
                    foreach (var point in points)
                    {
                        var position = point.centerPoint;
                        xMin = Math.Min(xMin, position.X);
                        xMax = Math.Max(xMax, position.X);
                        yMin = Math.Min(yMin, position.Y);
                        yMax = Math.Max(yMax, position.Y);
                        zMin = Math.Min(zMin, position.Z);
                        zMax = Math.Max(zMax, position.Z);
                    }
                    xLen = (xMax - xMin);
                    yLen = (yMax - yMin);
                    zLen = (zMax - zMin);
                    xCenter = (xMax + xMin) / 2;
                    yCenter = (yMax + yMin) / 2;
                    zCenter = (zMax + zMin) / 2;
                }
            }
        }

        private class Node
        {
            internal Node a;
            internal Node b;

            internal CenterPointedMesh[] points;
            internal BoundingBox bbox;
        }

        private class BSPToTree
        {
            internal Node Root { get; }

            public BSPToTree(BSP bsp)
            {
                Root = Walk(bsp);
            }

            private Node Walk(BSP bsp)
            {
                var pair = bsp.Split();
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
                    return new Node
                    {
                        points = pair[0].Points,

                        bbox = BoundingBox.FromManyPoints(
                            pair[0].Points
                                .Select(point => point.bigMesh)
                                .SelectMany(bigMesh => bigMesh.vertexList)
                        ),
                    };
                }
            }
        }
    }
}
