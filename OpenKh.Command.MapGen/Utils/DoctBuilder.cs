using OpenKh.Command.MapGen.Interfaces;
using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
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

        public List<SingleFace[]> vifPacketRenderingGroup { get; } = new List<SingleFace[]>();

        public DoctBuilder(ISpatialNodeCutter cutter)
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
            int entry2Idx = -1;

            if (node.faces != null)
            {
                entry2Idx = doct.Entry2List.Count;
                var entry2 = new Doct.Entry2
                {
                    BoundingBox = node.bbox,
                };
                doct.Entry2List.Add(entry2);

                vifPacketRenderingGroup.Add(
                    node.faces
                );
            }

            var entry1Idx = doct.Entry1List.Count;
            var entry1 = new Doct.Entry1();
            doct.Entry1List.Add(entry1);

            var childNodes = (node.children ?? new Node[0])
                .Select(it => WalkTree(it))
                .ToArray();

            entry1.Child1 = (short)(1 <= childNodes.Length ? childNodes[0].entry1Idx : -1);
            entry1.Child2 = (short)(2 <= childNodes.Length ? childNodes[1].entry1Idx : -1);
            entry1.Child3 = (short)(3 <= childNodes.Length ? childNodes[2].entry1Idx : -1);
            entry1.Child4 = (short)(4 <= childNodes.Length ? childNodes[3].entry1Idx : -1);
            entry1.Child5 = (short)(5 <= childNodes.Length ? childNodes[4].entry1Idx : -1);
            entry1.Child6 = (short)(6 <= childNodes.Length ? childNodes[5].entry1Idx : -1);
            entry1.Child7 = (short)(7 <= childNodes.Length ? childNodes[6].entry1Idx : -1);
            entry1.Child8 = (short)(8 <= childNodes.Length ? childNodes[7].entry1Idx : -1);
            entry1.BoundingBox = node.bbox;

            if (entry2Idx != -1)
            {
                entry1.Entry2Index = Convert.ToUInt16(entry2Idx);
                entry1.Entry2LastIndex = Convert.ToUInt16(entry2Idx + 1);
            };

            return new WalkResult
            {
                entry1Idx = Convert.ToUInt16(entry1Idx),
            };
        }

        private class Node
        {
            internal Node[] children;

            internal SingleFace[] faces;
            internal BoundingBox bbox;
        }

        private class ToTree
        {
            internal Node Root { get; }

            public ToTree(ISpatialNodeCutter cutter)
            {
                Root = Walk(cutter);
            }

            private Node Walk(ISpatialNodeCutter cutter)
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
                    var faces = pair.Single().Faces.ToArray();

                    return new Node
                    {
                        faces = faces,

                        bbox = BoundingBox.FromManyPoints(
                            faces
                                .SelectMany(point => point.positionList)
                        ),
                    };
                }
            }
        }
    }
}
